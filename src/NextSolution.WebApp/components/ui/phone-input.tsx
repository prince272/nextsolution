import React, { ChangeEvent, useEffect, useMemo, useState } from "react";
import { ChevronDownIcon } from "@/assets/icons";
import { Button, cn, Input, InputProps, Modal, ModalBody, ModalContent, ModalHeader, ModalProps, useDisclosure } from "@nextui-org/react";
import { AsYouType } from "libphonenumber-js";
import { CountryData, defaultCountries, FlagEmoji, parseCountry } from "react-international-phone";
import { Virtuoso } from "react-virtuoso";

import { useDebounce } from "@/lib/hooks";

export interface CountrySelectorModalProps extends Partial<Omit<ModalProps, "onSelect">> {
  onSelect?: (country: CountryData) => void;
  countries?: CountryData[];
}

const CountrySelectorModal: React.FC<CountrySelectorModalProps> = ({ onSelect, countries = defaultCountries, ...props }) => {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebounce<string>(search, 100);

  const organizedCountries = useMemo(() => {
    const getScore = (c: CountryData): number => {
      const country = parseCountry(c);
      const countryName = country.name.toLowerCase();
      const countryISO2 = country.iso2;

      const searchTerm = debouncedSearch.toLowerCase();

      if (countryName.startsWith(searchTerm) || countryISO2.startsWith(searchTerm)) {
        return 3; // Higher score for exact match or starts with
      } else if (countryName.includes(searchTerm) || countryISO2.includes(searchTerm)) {
        return 2; // Higher score for partial match
      } else if (countryName.includes(searchTerm.replace(/\s/g, "")) || countryISO2.includes(searchTerm.replace(/\s/g, ""))) {
        return 1; // Higher score for matches without spaces
      } else {
        return 0; // No match
      }
    };

    const organizedCountries = countries
      .slice()
      .map((country) => ({ country, score: getScore(country) }))
      .sort((a, b) => b.score - a.score)
      .filter((c) => c.score > 0)
      .map((c) => c.country);
    return organizedCountries;
  }, [debouncedSearch, countries]);

  return (
    <>
      <Modal scrollBehavior="inside" {...props} onClose={() => setSearch("")}>
        <ModalContent className="h-full">
          {(onClose) => (
            <>
              <ModalHeader className="flex flex-col gap-1">Select country</ModalHeader>
              <ModalBody className="px-0 pb-5">
                <div className="px-4">
                  <Input
                    type="text"
                    placeholder="Search country"
                    labelPlacement="outside"
                    label=""
                    isClearable
                    value={search}
                    onClear={() => setSearch("")}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                </div>
                <Virtuoso
                  className="flex h-full flex-col overflow-y-auto"
                  data={organizedCountries}
                  itemContent={(_, c) => {
                    const country = parseCountry(c);

                    return (
                      <Button
                        key={country.iso2}
                        as={"div"}
                        radius="none"
                        variant="light"
                        fullWidth
                        className={cn("justify-between px-5 py-5")}
                        onPress={() => {
                          onSelect?.(c);
                          setSearch("");
                          onClose();
                        }}
                      >
                        <div className="inline-flex gap-x-2">
                          <FlagEmoji iso2={country.iso2} disableLazyLoading style={{ marginRight: "8px" }} />
                          {country.name} ({country.iso2.toUpperCase()})
                        </div>
                        <div>+{country.dialCode}</div>
                      </Button>
                    );
                  }}
                />
              </ModalBody>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
};
CountrySelectorModal.displayName = "CountrySelectorModal";

export interface PhoneInputProps extends InputProps {
  countries?: CountryData[];
}

const PhoneInput = React.forwardRef<HTMLInputElement, PhoneInputProps>(({ value: nativeValue, onChange: onNativeChange, countries = defaultCountries, ...props }, ref) => {
  const { onOpen: openCountrySelector, isOpen: isCountrySelectorOpen, onOpenChange: onCountrySelectorOpenChange } = useDisclosure();

  const { defaultCountry, defaultValue } = useMemo(() => {
    const asYouType = new AsYouType();
    asYouType.input(nativeValue as string);

    const defaultCountry = countries.map(parseCountry).find((c) => c.iso2 == asYouType.getCountry()?.toLowerCase()) ?? parseCountry(countries[0]);
    const defaultValue = checkPhoneNumber(nativeValue) ? asYouType.getNumber()?.formatNational() : nativeValue;
    return { defaultCountry, defaultValue };
  }, [countries, nativeValue]);

  const [country, setCountry] = useState(defaultCountry);
  const [value, setValue] = useState(defaultValue);

  useEffect(() => {
    const asYouType = new AsYouType({ defaultCallingCode: country.dialCode, defaultCountry: country.iso2.toLowerCase() as any });
    asYouType.input(value as string);

    const newNativeValue = checkPhoneNumber(value) ? asYouType.getNumberValue() ?? value : value;
    onNativeChange?.({ target: { value: newNativeValue } as HTMLInputElement } as ChangeEvent<HTMLInputElement>);
  }, [value, onNativeChange, country.iso2, country.dialCode]);

  return (
    <Input
      value={value}
      onChange={(e) => setValue(e.target.value)}
      endContent={
        <>
          <Button className={cn("px-5", !checkPhoneNumber(value) && "hidden")} color="default" variant="faded" size="sm" type="button" onPress={openCountrySelector}>
            <FlagEmoji iso2={country.iso2} className="inline-flex" /> <div className="inline-flex">+{country.dialCode}</div>{" "}
            <ChevronDownIcon className="inline-flex h-4 w-4 flex-none text-default-400" />
          </Button>
          <CountrySelectorModal
            isOpen={isCountrySelectorOpen}
            onOpenChange={onCountrySelectorOpenChange}
            onSelect={(selectedCountry) => {
              setCountry(parseCountry(selectedCountry));
            }}
          />
        </>
      }
      {...props}
      ref={ref}
    />
  );
});
PhoneInput.displayName = "PhoneInput";

export { PhoneInput };

export const checkPhoneNumber = (value: any) => {
  const isEmpty = (value: any) => {
    return typeof value === "undefined" || value == null || value.replace(/\s/g, "").length < 1;
  };

  if (isEmpty(value)) return false;

  const result = new RegExp("^[ 0-9.,+\\-()]*$").test(value || "");
  return result;
};
