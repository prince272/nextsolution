export const GoogleIcon: React.FC<React.HTMLAttributes<SVGAElement>> = (props: any) => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48" {...props}>
    <path
      fill="#FFC107"
      d="M43.611,20.083H42V20H24v8h11.303c-1.649,4.657-6.08,8-11.303,8c-6.627,0-12-5.373-12-12c0-6.627,5.373-12,12-12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C12.955,4,4,12.955,4,24c0,11.045,8.955,20,20,20c11.045,0,20-8.955,20-20C44,22.659,43.862,21.35,43.611,20.083z"
    />
    <path
      fill="#FF3D00"
      d="M6.306,14.691l6.571,4.819C14.655,15.108,18.961,12,24,12c3.059,0,5.842,1.154,7.961,3.039l5.657-5.657C34.046,6.053,29.268,4,24,4C16.318,4,9.656,8.337,6.306,14.691z"
    />
    <path
      fill="#4CAF50"
      d="M24,44c5.166,0,9.86-1.977,13.409-5.192l-6.19-5.238C29.211,35.091,26.715,36,24,36c-5.202,0-9.619-3.317-11.283-7.946l-6.522,5.025C9.505,39.556,16.227,44,24,44z"
    />
    <path
      fill="#1976D2"
      d="M43.611,20.083H42V20H24v8h11.303c-0.792,2.237-2.231,4.166-4.087,5.571c0.001-0.001,0.002-0.001,0.003-0.002l6.19,5.238C36.971,39.205,44,34,44,24C44,22.659,43.862,21.35,43.611,20.083z"
    />
  </svg>
);

export const PersonIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24" {...props}>
        <path
          d="M17.754 14a2.249 2.249 0 0 1 2.25 2.249v.918a2.75 2.75 0 0 1-.513 1.599C17.945 20.929 15.42 22 12 22c-3.422 0-5.945-1.072-7.487-3.237a2.75 2.75 0 0 1-.51-1.595v-.92a2.249 2.249 0 0 1 2.249-2.25h11.501ZM12 2.004a5 5 0 1 1 0 10 5 5 0 0 1 0-10Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24" {...props}>
        <path
          d="M17.754 14a2.249 2.249 0 0 1 2.25 2.249v.575c0 .894-.32 1.76-.902 2.438-1.57 1.834-3.957 2.739-7.102 2.739-3.146 0-5.532-.905-7.098-2.74a3.75 3.75 0 0 1-.898-2.435v-.577a2.249 2.249 0 0 1 2.249-2.25h11.501Zm0 1.5H6.253a.749.749 0 0 0-.75.749v.577c0 .536.192 1.054.54 1.461 1.253 1.468 3.219 2.214 5.957 2.214s4.706-.746 5.962-2.214a2.25 2.25 0 0 0 .541-1.463v-.575a.749.749 0 0 0-.749-.75ZM12 2.004a5 5 0 1 1 0 10 5 5 0 0 1 0-10Zm0 1.5a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const EyeShowIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M12 9.005a4 4 0 1 1 0 8 4 4 0 0 1 0-8ZM12 5.5c4.613 0 8.596 3.15 9.701 7.564a.75.75 0 1 1-1.455.365 8.503 8.503 0 0 0-16.493.004.75.75 0 0 1-1.455-.363A10.003 10.003 0 0 1 12 5.5Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M12 9.005a4 4 0 1 1 0 8 4 4 0 0 1 0-8Zm0 1.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5ZM12 5.5c4.613 0 8.596 3.15 9.701 7.564a.75.75 0 1 1-1.455.365 8.503 8.503 0 0 0-16.493.004.75.75 0 0 1-1.455-.363A10.003 10.003 0 0 1 12 5.5Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const EyeHideIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M2.22 2.22a.75.75 0 0 0-.073.976l.073.084 4.034 4.035a9.986 9.986 0 0 0-3.955 5.75.75.75 0 0 0 1.455.364 8.49 8.49 0 0 1 3.58-5.034l1.81 1.81A4 4 0 0 0 14.8 15.86l5.919 5.92a.75.75 0 0 0 1.133-.977l-.073-.084-6.113-6.114.001-.002-6.95-6.946.002-.002-1.133-1.13L3.28 2.22a.75.75 0 0 0-1.06 0ZM12 5.5c-1 0-1.97.148-2.889.425l1.237 1.236a8.503 8.503 0 0 1 9.899 6.272.75.75 0 0 0 1.455-.363A10.003 10.003 0 0 0 12 5.5Zm.195 3.51 3.801 3.8a4.003 4.003 0 0 0-3.801-3.8Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M2.22 2.22a.75.75 0 0 0-.073.976l.073.084 4.034 4.035a9.986 9.986 0 0 0-3.955 5.75.75.75 0 0 0 1.455.364 8.49 8.49 0 0 1 3.58-5.034l1.81 1.81A4 4 0 0 0 14.8 15.86l5.919 5.92a.75.75 0 0 0 1.133-.977l-.073-.084-6.113-6.114.001-.002-6.95-6.946.002-.002-1.133-1.13L3.28 2.22a.75.75 0 0 0-1.06 0ZM12 5.5c-1 0-1.97.148-2.889.425l1.237 1.236a8.503 8.503 0 0 1 9.899 6.272.75.75 0 0 0 1.455-.363A10.003 10.003 0 0 0 12 5.5Zm.195 3.51 3.801 3.8a4.003 4.003 0 0 0-3.801-3.8Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const ChevronUpIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M4.293 15.707a1 1 0 0 0 1.414 0L12 9.414l6.293 6.293a1 1 0 0 0 1.414-1.414l-7-7a1 1 0 0 0-1.414 0l-7 7a1 1 0 0 0 0 1.414Z" fill="currentColor" />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M4.293 15.707a1 1 0 0 0 1.414 0L12 9.414l6.293 6.293a1 1 0 0 0 1.414-1.414l-7-7a1 1 0 0 0-1.414 0l-7 7a1 1 0 0 0 0 1.414Z" fill="currentColor" />
      </svg>
    )
  })[type];

export const ChevronDownIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M4.293 8.293a1 1 0 0 1 1.414 0L12 14.586l6.293-6.293a1 1 0 1 1 1.414 1.414l-7 7a1 1 0 0 1-1.414 0l-7-7a1 1 0 0 1 0-1.414Z" fill="currentColor" />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M4.22 8.47a.75.75 0 0 1 1.06 0L12 15.19l6.72-6.72a.75.75 0 1 1 1.06 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L4.22 9.53a.75.75 0 0 1 0-1.06Z" fill="currentColor" />
      </svg>
    )
  })[type];

export const ChevronLeftIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M15.707 4.293a1 1 0 0 1 0 1.414L9.414 12l6.293 6.293a1 1 0 0 1-1.414 1.414l-7-7a1 1 0 0 1 0-1.414l7-7a1 1 0 0 1 1.414 0Z" fill="currentColor" />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M15.53 4.22a.75.75 0 0 1 0 1.06L8.81 12l6.72 6.72a.75.75 0 1 1-1.06 1.06l-7.25-7.25a.75.75 0 0 1 0-1.06l7.25-7.25a.75.75 0 0 1 1.06 0Z" fill="currentColor" />
      </svg>
    )
  })[type];

export const ChevronRightIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M8.293 4.293a1 1 0 0 0 0 1.414L14.586 12l-6.293 6.293a1 1 0 1 0 1.414 1.414l7-7a1 1 0 0 0 0-1.414l-7-7a1 1 0 0 0-1.414 0Z" fill="currentColor" />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path d="M8.47 4.22a.75.75 0 0 0 0 1.06L15.19 12l-6.72 6.72a.75.75 0 1 0 1.06 1.06l7.25-7.25a.75.75 0 0 0 0-1.06L9.53 4.22a.75.75 0 0 0-1.06 0Z" fill="currentColor" />
      </svg>
    )
  })[type];

export const DismissIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="m4.21 4.387.083-.094a1 1 0 0 1 1.32-.083l.094.083L12 10.585l6.293-6.292a1 1 0 1 1 1.414 1.414L13.415 12l6.292 6.293a1 1 0 0 1 .083 1.32l-.083.094a1 1 0 0 1-1.32.083l-.094-.083L12 13.415l-6.293 6.292a1 1 0 0 1-1.414-1.414L10.585 12 4.293 5.707a1 1 0 0 1-.083-1.32l.083-.094-.083.094Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="m4.397 4.554.073-.084a.75.75 0 0 1 .976-.073l.084.073L12 10.939l6.47-6.47a.75.75 0 1 1 1.06 1.061L13.061 12l6.47 6.47a.75.75 0 0 1 .072.976l-.073.084a.75.75 0 0 1-.976.073l-.084-.073L12 13.061l-6.47 6.47a.75.75 0 0 1-1.06-1.061L10.939 12l-6.47-6.47a.75.75 0 0 1-.072-.976l.073-.084-.073.084Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const AddIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M11.883 3.007 12 3a1 1 0 0 1 .993.883L13 4v7h7a1 1 0 0 1 .993.883L21 12a1 1 0 0 1-.883.993L20 13h-7v7a1 1 0 0 1-.883.993L12 21a1 1 0 0 1-.993-.883L11 20v-7H4a1 1 0 0 1-.993-.883L3 12a1 1 0 0 1 .883-.993L4 11h7V4a1 1 0 0 1 .883-.993L12 3l-.117.007Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M11.75 3a.75.75 0 0 1 .743.648l.007.102.001 7.25h7.253a.75.75 0 0 1 .102 1.493l-.102.007h-7.253l.002 7.25a.75.75 0 0 1-1.493.101l-.007-.102-.002-7.249H3.752a.75.75 0 0 1-.102-1.493L3.752 11h7.25L11 3.75a.75.75 0 0 1 .75-.75Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const DockPanelLeftIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M4.75 4A2.75 2.75 0 0 0 2 6.75v10.5A2.75 2.75 0 0 0 4.75 20h14.5A2.75 2.75 0 0 0 22 17.25V6.75A2.75 2.75 0 0 0 19.25 4H4.75ZM9 18.5v-13h10.25c.69 0 1.25.56 1.25 1.25v10.5c0 .69-.56 1.25-1.25 1.25H9Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M4.75 4A2.75 2.75 0 0 0 2 6.75v10.5A2.75 2.75 0 0 0 4.75 20h14.5A2.75 2.75 0 0 0 22 17.25V6.75A2.75 2.75 0 0 0 19.25 4H4.75ZM3.5 6.75c0-.69.56-1.25 1.25-1.25h3.254v13H4.75c-.69 0-1.25-.56-1.25-1.25V6.75ZM9.504 18.5v-13h9.746c.69 0 1.25.56 1.25 1.25v10.5c0 .69-.56 1.25-1.25 1.25H9.504Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const DockPanelRightIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M19.25 4A2.75 2.75 0 0 1 22 6.75v10.5A2.75 2.75 0 0 1 19.25 20H4.75A2.75 2.75 0 0 1 2 17.25V6.75A2.75 2.75 0 0 1 4.75 4h14.5ZM15 18.5v-13H4.75c-.69 0-1.25.56-1.25 1.25v10.5c0 .69.56 1.25 1.25 1.25H15Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M19.25 4A2.75 2.75 0 0 1 22 6.75v10.5A2.75 2.75 0 0 1 19.25 20H4.75A2.75 2.75 0 0 1 2 17.25V6.75A2.75 2.75 0 0 1 4.75 4h14.5Zm1.25 2.75c0-.69-.56-1.25-1.25-1.25h-3.254v13h3.254c.69 0 1.25-.56 1.25-1.25V6.75ZM14.496 18.5v-13H4.75c-.69 0-1.25.56-1.25 1.25v10.5c0 .69.56 1.25 1.25 1.25h9.746Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const NavigationIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M3 17h18a1 1 0 0 1 .117 1.993L21 19H3a1 1 0 0 1-.117-1.993L3 17h18H3Zm0-6 18-.002a1 1 0 0 1 .117 1.993l-.117.007L3 13a1 1 0 0 1-.117-1.993L3 11l18-.002L3 11Zm0-6h18a1 1 0 0 1 .117 1.993L21 7H3a1 1 0 0 1-.117-1.993L3 5h18H3Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
        <path
          d="M2.753 18h18.5a.75.75 0 0 1 .102 1.493l-.102.007h-18.5a.75.75 0 0 1-.102-1.493L2.753 18h18.5-18.5Zm0-6.497h18.5a.75.75 0 0 1 .102 1.493l-.102.007h-18.5a.75.75 0 0 1-.102-1.493l.102-.007h18.5-18.5Zm-.001-6.5h18.5a.75.75 0 0 1 .102 1.493l-.102.007h-18.5A.75.75 0 0 1 2.65 5.01l.102-.007h18.5-18.5Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];

export const SendIcon: React.FC<React.SVGProps<SVGSVGElement> & { type?: "filled" | "outlined" }> = ({ type = "filled", ...props }) =>
  ({
    filled: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="m12.815 12.197-7.532 1.256a.5.5 0 0 0-.386.318L2.3 20.728c-.248.64.421 1.25 1.035.943l18-9a.75.75 0 0 0 0-1.342l-18-9c-.614-.307-1.283.304-1.035.943l2.598 6.957a.5.5 0 0 0 .386.319l7.532 1.255a.2.2 0 0 1 0 .394Z"
          fill="currentColor"
        />
      </svg>
    ),
    outlined: (
      <svg fill="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" {...props}>
        <path
          d="M5.694 12 2.299 3.272c-.236-.607.356-1.188.942-.982l.093.04 18 9a.75.75 0 0 1 .097 1.283l-.097.058-18 9c-.583.291-1.217-.244-1.065-.847l.03-.096L5.694 12 2.299 3.272 5.694 12ZM4.402 4.54l2.61 6.71h6.627a.75.75 0 0 1 .743.648l.007.102a.75.75 0 0 1-.649.743l-.101.007H7.01l-2.609 6.71L19.322 12 4.401 4.54Z"
          fill="currentColor"
        />
      </svg>
    )
  })[type];
