using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NextSolution.Core.Shared;
using NextSolution.Core.Helpers;

namespace NextSolution.Infrastructure.Data.Extensions
{
    public static class ModalBuilderExtensions
    {
        public static ModelBuilder ApplyEntities(this ModelBuilder modelBuilder, Func<Type, bool>? predicate = null)
        {
            var entityTypes = TypeHelper.GetTypesFromApplicationDependencies()
                .Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.IsCompatibleWith(typeof(IEntity)) && (predicate?.Invoke(type) ?? true));

            foreach (var entityType in entityTypes)
            {
                modelBuilder.Entity(entityType);
            }

            return modelBuilder;
        }

        public static ModelBuilder ApplyConfigurations(this ModelBuilder modelBuilder, Func<Type, bool>? predicate = null)
        {
            var entityTypeConfigurationTypes = TypeHelper.GetTypesFromApplicationDependencies()
                .Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.IsCompatibleWith(typeof(IEntityTypeConfiguration<>)) && (predicate?.Invoke(type) ?? true));

            var applyEntityConfigurationMethod = typeof(ModelBuilder)
                .GetMethods()
                .Single(
                    e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                        && e.ContainsGenericParameters
                        && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
                        == typeof(IEntityTypeConfiguration<>));

            foreach (var entityTypeConfigurationType in entityTypeConfigurationTypes)
            {
                // Only accept types that contain a parameterless constructor, are not abstract and satisfy a predicate if it was used.
                if (entityTypeConfigurationType.GetConstructor(Type.EmptyTypes) == null
                    || (!predicate?.Invoke(entityTypeConfigurationType) ?? false))
                {
                    continue;
                }

                foreach (var @interface in entityTypeConfigurationType.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    if (@interface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var target = applyEntityConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(entityTypeConfigurationType) });
                    }
                }
            }

            return modelBuilder;
        }

    }
}
