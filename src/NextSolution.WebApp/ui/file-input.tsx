"use client";

import { forwardRef, useCallback, useEffect, useRef, useState } from "react";
import FilePondPluginImagePreview from "filepond-plugin-image-preview";
import { FilePond, FilePondProps, registerPlugin } from "react-filepond";

import "filepond/dist/filepond.min.css";
import "filepond-plugin-image-preview/dist/filepond-plugin-image-preview.css";

import { FilePondCallbackProps, FilePondFile, FilePondInitialFile, FilePondServerConfigProps, FilePondStyleProps } from "filepond";
import toast from "react-hot-toast";
import { v4 as uuidv4 } from "uuid";

import { useApi, useUser } from "@/lib/api/client";
import { getApiErrorMessage } from "@/lib/api/utils";

registerPlugin(FilePondPluginImagePreview);

export interface FileInputProps extends Omit<FilePondProps, keyof FilePondCallbackProps | "server"> {
  value?: string | string[];
  onChange?: (value: string | null | string[]) => void;
  variant?: "circle" | "rectangle";
  endpoint: string;
}

const FileInput = forwardRef<FilePond, FileInputProps>(({ value, onChange, endpoint, variant = "rectangle", ...props }, ref) => {
  const api = useApi();
  const currentUser = useUser();
  const componentId = useRef(uuidv4()).current;
  const filesRef = useRef<(FilePondFile & FilePondInitialFile)[]>([]);
  const fileTransferIdRef = useRef<string | null>();

  const loadedFiles = (Array.isArray(value) ? value : `${value ? value : ""}`.split(",").filter((source) => source)).map(
    (source) => (filesRef.current.find((file) => file.serverId == source) || { source, options: { type: "local" } }) as FilePondInitialFile
  );

  const handleChange = () => {
    const files = filesRef.current;
    const newValues = files.filter((file) => file.serverId).map((file) => file.serverId);
    const newValue = newValues.join(",").replace(/,$/, "") || null;
    onChange?.(Array.isArray(value) ? newValues : newValue);
  };

  const serverProps: FilePondServerConfigProps | { [key: string]: any } = {
    server: {
      url: api.config.baseURL,
      process: {
        url: endpoint.replace(/\s+$/, "") + "/",
        method: "POST",
        withCredentials: api.config.withCredentials,
        headers: ((file: File, form: any) => {
          const headers: { [key: string]: string | boolean | number } = {};

          if (file) {
            headers["Upload-Id"] = fileTransferIdRef.current = uuidv4();
            headers["Upload-Name"] = file.name;
            headers["Upload-Length"] = file.size;
            headers["Upload-Type"] = file.type;
            headers["Upload-Offset"] = 0;
          }

          if (currentUser) headers["Authorization"] = `${currentUser.tokenType} ${currentUser.accessToken}`;
          return headers;
        }) as any,
        onerror: (response) => {
          const error = JSON.parse(response);
          toast.error(getApiErrorMessage(error), { id: componentId });
        }
      },
      patch: {
        url: endpoint.replace(/\s+$/, "") + "/",
        method: "PATCH",
        withCredentials: api.config.withCredentials,
        headers: ({ file, offset }: { file: File; offset: number }) => {
          const headers: { [key: string]: string | boolean | number } = {};

          if (file) {
            headers["Upload-Id"] = fileTransferIdRef.current!;
            headers["Upload-Name"] = file.name;
            headers["Upload-Length"] = file.size;
            headers["Upload-Type"] = file.type;
            headers["Upload-Offset"] = offset;
          }

          if (currentUser) headers["Authorization"] = `${currentUser.tokenType} ${currentUser.accessToken}`;
          return headers;
        },
        onerror: (response: any) => {
          const error = JSON.parse(response);
          toast.error(getApiErrorMessage(error), { id: componentId });
        }
      },
      revert: (uniqueFieldId: any, load: () => void, error: (errorText: string) => void) => {
        // Helper Functions for Making XHR Requests in JavaScript
        // source: https://gist.github.com/pizzarob/6c9efc583a17c2643505e7d70ffb1e0e
        let xhr = new XMLHttpRequest();
        xhr.withCredentials = !!api.config.withCredentials;
        xhr.open("DELETE", new URL(endpoint.replace(/\s+$/, "") + "/" + uniqueFieldId, api.config.baseURL).toString());
        xhr.setRequestHeader("Content-Type", "application/offset+octet-stream");

        if (currentUser) {
          xhr.setRequestHeader("Authorization", `${currentUser.tokenType} ${currentUser.accessToken}`);
        }

        xhr.addEventListener("load", () => {
          let { responseText, status } = xhr;
          if (status >= 200 && status < 400) {
            load();
          } else {
            error(responseText);
          }
        });

        xhr.send();
      },
      load: {
        url: endpoint.replace(/\s+$/, "") + "/",
        headers: {
          ...(!currentUser ? {} : { Authorization: `${currentUser.tokenType} ${currentUser.accessToken}` })
        }
      }
    },
    chunkUploads: true,
    chunkForce: true
  };

  const styleProps: FilePondStyleProps =
    variant == "circle"
      ? {
          stylePanelLayout: "compact circle",
          styleLoadIndicatorPosition: "center bottom",
          styleProgressIndicatorPosition: "right bottom",
          styleButtonRemoveItemPosition: "left bottom",
          styleButtonProcessItemPosition: "right bottom"
        }
      : {};

  return (
    <>
      <FilePond
        files={loadedFiles}
        onupdatefiles={(files) => {
          filesRef.current = files as any;
          handleChange();
        }}
        onreorderfiles={(files) => {
          filesRef.current = files as any;
          handleChange();
        }}
        onprocessfile={() => {
          handleChange();
        }}
        onremovefile={() => {
          handleChange();
        }}
        chunkUploads={true}
        chunkForce={true}
        credits={false}
        {...serverProps}
        {...styleProps}
        {...props}
        ref={ref}
      />
      <style jsx global>{`
        /* use a hand cursor intead of arrow for the action buttons */
        .filepond--file-action-button {
          cursor: pointer;
        }

        /* the text color of the drop label*/
        .filepond--drop-label {
          color: hsl(var(--nextui-foreground-600) / var(--nextui-foreground-600-opacity, var(--tw-text-opacity)));
          font-size: var(--nextui-font-size-small) !important;
        }

        /* underline color for "Browse" button */
        .filepond--label-action {
          text-decoration-color: hsl(var(--nextui-foreground-600) / var(--nextui-foreground-600-opacity, var(--tw-text-opacity)));
          font-size: var(--nextui-font-size-small) !important;
        }

        /* the background color of the filepond drop area */
        .filepond--panel-root {
          background-color: #eee;
        }

        /* the border radius of the drop area */
        .filepond--panel-root {
          border-radius: 0.5em;
        }

        /* the border radius of the file item */
        .filepond--item-panel {
          border-radius: 0.5em;
        }

        /* the background color of the file and file panel (used when dropping an image) */
        .filepond--item-panel {
          background-color: #555;
        }

        /* the background color of the drop circle */
        .filepond--drip-blob {
          background-color: #999;
        }

        /* the background color of the black action buttons */
        .filepond--file-action-button {
          background-color: rgba(0, 0, 0, 0.5);
        }

        /* the icon color of the black action buttons */
        .filepond--file-action-button {
          color: white;
        }

        /* the color of the focus ring */
        .filepond--file-action-button:hover,
        .filepond--file-action-button:focus {
          box-shadow: 0 0 0 0.125em rgba(255, 255, 255, 0.9);
        }

        /* the text color of the file status and info labels */
        .filepond--file {
          color: white;
        }

        /* error state color */
        [data-filepond-item-state*="error"] .filepond--item-panel,
        [data-filepond-item-state*="invalid"] .filepond--item-panel {
          background-color: hsl(var(--nextui-danger) / var(--nextui-danger-opacity, var(--tw-bg-opacity)));
        }

        [data-filepond-item-state="processing-complete"] .filepond--item-panel {
          background-color: hsl(var(--nextui-success) / var(--nextui-success-opacity, var(--tw-bg-opacity)));
        }

        /* bordered drop area */
        .filepond--panel-root {
          background-color: hsl(var(--nextui-default-100) / var(--nextui-default-100-opacity, var(--tw-bg-opacity)));
        }

        /* image preview error overlay */
        .filepond--image-preview-overlay-failure {
          color: hsl(var(--nextui-danger) / var(--nextui-danger-opacity, var(--tw-bg-opacity)));
        }

        .filepond--image-preview-overlay-success {
          color: hsl(var(--nextui-success) / var(--nextui-success-opacity, var(--tw-bg-opacity)));
        }

        .filepond--root {
          margin-bottom: 0px !important;
        }
      `}</style>
    </>
  );
});
FileInput.displayName = "FileInput";

export { FileInput };
