import * as React from "react";

type PinInputValues = string[];

export interface PinInputActions {
  /**
   * Imperative function call to lose focus.
   */
  blur: () => void;
  /**
   * Imperative function call to set focus on the first empty field. In the case when `error: true`,
   * the focus is set to the first empty field. The argument takes an optional parameter in the form
   * of a number (ordinal index), which will set the focus on a specific field.
   * @param {number} index
   */
  focus: (index?: number) => void;
}

export interface UsePinInputProps {
  /**
   * Field values. If no values are passed, the default value defined in `defaultValues` is used.
   */
  values?: PinInputValues;
  /**
   * The function is called every time the value changes.
   * @param {PinInputValues} values
   */
  onChange?: (values: PinInputValues) => void;
  /**
   * The function is called when all fields are filled in.
   * @param {string} value
   */
  onComplete?: (value: string) => void;
  /**
   * A reference to imperative actions.
   */
  actionRef?: React.Ref<PinInputActions>;
  /**
   * Automatic focus setting at the first mount, is set to the first field.
   */
  autoFocus?: boolean;
  /**
   * In the case when the component is unmanaged, default values are set, where the number of values
   * in the array is equal to the number of fields.
   */
  defaultValues?: PinInputValues;
  /**
   * Changes the type of keyboard display on mobile devices.
   */
  type?: "numeric" | "alphanumeric";
  /**
   * If `true', then the attribute `autocomplete="one-time-code"` is added, otherwise
   * `autocomplete="off"`.
   */
  otp?: boolean;
  /**
   * Placeholder for the `input` element.
   */
  placeholder?: string;
  /**
   * If `true`, the transmission of event handlers `onChange`, `onBlur`, `onFocus` and `onKeyDown`
   * in the parameters of each field is prevented.
   */
  disabled?: boolean;
  /**
   * If `true`, then the attribute `type="password"` is set, otherwise `type="text"`.
   */
  mask?: boolean;
  /**
   * If `true`, then the focus behavior on the fields changes.
   */
  error?: boolean;
}

export type PinInputClearOptions = {
  /**
   * If `true', then the focus is set on the first field, otherwise after clearing it disappears.
   */
  focus?: boolean;
};

export interface PinInputFieldProps {
  /**
   * Returns a callback function to register the field.
   */
  ref: React.RefCallback<HTMLInputElement>;
  /**
   * Returns the value of the field.
   */
  value: string;
  /**
   * Returns the value passed by the `disabled` parameter.
   */
  disabled: boolean;
  /**
   * Returns either `one-time-code` if the parameter `otp: true` was passed, otherwise `off`.
   */
  autoComplete: "one-time-code" | "off";
  /**
   * Returns either `text` if the parameter `type: 'alphanumeric'` was passed, otherwise `numeric`.
   */
  inputMode: "text" | "numeric";
  /**
   * Returns either `password` if the `mask: true` parameter was passed, or `text`.
   */
  type: "text" | "password";
  /**
   * Returns the value passed by the `placeholder` parameter if there are no focused fields.
   */
  placeholder: string;
  /**
   * Returns the handler for the focus loss event if `disabled: false`.
   * @param {React.FocusEvent<HTMLInputElement>} event
   */
  onBlur?: React.FocusEventHandler<HTMLInputElement>;
  /**
   * Returns the handler for the focus appearance event if `disabled: false`.
   * @param {React.FocusEvent<HTMLInputElement>} event
   */
  onFocus?: React.FocusEventHandler<HTMLInputElement>;
  /**
   * Returns a handler for the field change event if `disabled: false`.
   * @param {React.ChangeEvent<HTMLInputElement>} event
   */
  onChange?: React.ChangeEventHandler<HTMLInputElement>;
  /**
   * Returns a handler for the keystroke event if `disabled: false`.
   * @param {React.KeyboardEvent<HTMLInputElement>} event
   */
  onKeyDown?: React.KeyboardEventHandler<HTMLInputElement>;
}

export function usePinInput({
  values: valuesProp,
  onChange: onChangeProp,
  onComplete,
  actionRef,
  autoFocus = false,
  defaultValues = Array(6).fill(""),
  type = "numeric",
  otp = false,
  placeholder = "○",
  disabled = false,
  mask = false,
  error = false
}: UsePinInputProps = {}) {
  const [valuesState, setValues] = React.useState(defaultValues);
  const [focusedIndex, setFocusedIndex] = React.useState(-1);

  const isControlled = valuesProp !== undefined;
  const values = (isControlled ? valuesProp : valuesState) as PinInputValues;
  const isTypeAlphanumeric = type === "alphanumeric";

  const fieldRefs = React.useRef<HTMLInputElement[]>(Array(values.length).fill(null));

  const setFocus = React.useCallback((index = 0) => {
    fieldRefs.current[index]?.focus();
  }, []);

  React.useEffect(() => {
    if (autoFocus) {
      setFocus();
    }
  }, [autoFocus, setFocus]);

  const setBlur = React.useCallback(() => {
    fieldRefs.current[focusedIndex]?.blur();
  }, [focusedIndex]);

  React.useImperativeHandle(
    actionRef,
    () => ({
      focus: (index = 0) => {
        const emptyFieldIndex = values.findIndex((v) => !v);

        setFocus(error ? (emptyFieldIndex === -1 ? index : emptyFieldIndex) : index);
      },
      blur: setBlur
    }),
    [setBlur, setFocus, values, error]
  );

  const setFieldRef = React.useCallback(
    (index: number) => (ref: HTMLInputElement) => {
      fieldRefs.current[index] = ref;
    },
    []
  );

  const updateValues = React.useCallback(
    (values: PinInputValues) => {
      if (!isControlled) {
        setValues(values);
      }

      onChangeProp?.(values);
    },
    [isControlled, setValues, onChangeProp]
  );

  const onChange = React.useCallback(
    (index: number): React.ChangeEventHandler<HTMLInputElement> =>
      (event) => {
        let { value } = event.target;

        value = value.trim();

        const regexType = isTypeAlphanumeric ? /^[a-z\d]*$/i : /^\d*$/;

        if (!regexType.test(value)) {
          return;
        }

        if (isTypeAlphanumeric) {
          value = value.toUpperCase();
        }

        if (value.length > 2) {
          if (value.length === values.length) {
            updateValues(value.split(""));
            onComplete?.(value);
          }

          return;
        }

        if (value.length === 2) {
          const currentValue = values[index];

          if (currentValue === value[0]) {
            value = value[1];
          } else if (currentValue === value[1]) {
            value = value[0];
          } else {
            return;
          }
        }

        const nextValues = values.slice();
        nextValues[index] = value;
        updateValues(nextValues);

        if (value) {
          if (!nextValues.includes("")) {
            onComplete?.(nextValues.join(""));
          }

          if (index !== values.length - 1) {
            if (error) {
              const emptyFieldIndex = nextValues.findIndex((v) => !v);

              if (emptyFieldIndex !== -1) {
                setFocus(emptyFieldIndex);
              }
            } else {
              setFocus(index + 1);
            }
          }
        }
      },
    [isTypeAlphanumeric, values, updateValues, onComplete, setFocus, error]
  );

  const onKeyDown = React.useCallback(
    (index: number): React.KeyboardEventHandler<HTMLInputElement> =>
      (event) => {
        if (event.key === "Backspace" && !values[index] && index) {
          setFocus(index - 1);
        }
      },
    [values, setFocus]
  );

  const clear = React.useCallback(
    ({ focus = false }: PinInputClearOptions = {}) => {
      updateValues(Array(values.length).fill(""));

      if (focus) {
        setFocus();
      } else {
        setBlur();
      }
    },
    [updateValues, values, setFocus, setBlur]
  );

  const onFocus = React.useCallback(
    (index: number) => () => {
      setFocusedIndex(index);
    },
    []
  );

  const onBlur = React.useCallback(() => {
    setFocusedIndex(-1);
  }, []);

  const hasFocus = focusedIndex !== -1;

  const fields: PinInputFieldProps[] = values.map((value: string, index: number) => ({
    ref: setFieldRef(index),
    value,
    disabled,
    autoComplete: otp ? "one-time-code" : "off",
    inputMode: isTypeAlphanumeric ? "text" : "numeric",
    type: mask ? "password" : "text",
    placeholder: hasFocus ? "" : placeholder,
    ...(!disabled && {
      onBlur,
      onFocus: onFocus(index),
      onChange: onChange(index),
      onKeyDown: onKeyDown(index)
    })
  }));

  return { fields, clear, isFocused: hasFocus };
}

export type UsePinInputReturn = ReturnType<typeof usePinInput>;
