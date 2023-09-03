import React from "react";
import FilePondPluginImagePreview from "filepond-plugin-image-preview";
import { FilePond, FilePondProps, registerPlugin } from "react-filepond";

import "filepond/dist/filepond.min.css";
import "filepond-plugin-image-preview/dist/filepond-plugin-image-preview.css";

import { FilePondCallbackProps, FilePondInitialFile, FilePondServerConfigProps } from "filepond";
import { isString } from "lodash";

import { useApi, useUser } from "@/lib/api";

registerPlugin(FilePondPluginImagePreview);
export interface FileInputProps extends Omit<FilePondProps, keyof FilePondCallbackProps> {
  value?: string | string[];
  onChange?: (value: string | string[]) => void;
}

const FileInput = React.forwardRef<FilePond, FileInputProps>(({ value, onChange, server, ...props }, ref) => {
  const api = useApi();
  const currentUser = useUser();

  const initialFiles = (Array.isArray(value) ? value.filter((source) => source) : [value as string].filter((source) => source)).map((source) => ({
    source,
    options: { type: "local" }
  })) as FilePondInitialFile[];

  const serverProps: FilePondServerConfigProps | { [key: string]: any } = {
    server: isString(server)
      ? {
          url: new URL(server, api.config.baseURL).toString(),
          process: {
            url: "/",
            method: "POST",
            withCredentials: api.config.withCredentials,
            headers: ((file: File) => {
              const headers: { [key: string]: string | boolean | number } = {};

              if (file) {
                headers["Upload-Name"] = file.name;
                headers["Upload-Length"] = file.size;
                headers["Upload-Type"] = file.type;
                headers["Upload-Offset"] = 0;
              }

              if (currentUser) headers["Authorization"] = `${currentUser.tokenType} ${currentUser.accessToken}`;
              return headers;
            }) as any
          },
          patch: {
            url: "/",
            method: "PATCH",
            withCredentials: api.config.withCredentials,
            headers: ({ file, offset }: { file: File; offset: number }) => {
              const headers: { [key: string]: string | boolean | number } = {};

              if (file) {
                headers["Upload-Name"] = file.name;
                headers["Upload-Length"] = file.size;
                headers["Upload-Type"] = file.type;
                headers["Upload-Offset"] = offset;
              }

              if (currentUser) headers["Authorization"] = `${currentUser.tokenType} ${currentUser.accessToken}`;
              return headers;
            }
          },
          revert: (uniqueFieldId: any, load: () => void, error: (errorText: string) => void) => {
            // Helper Functions for Making XHR Requests in JavaScript
            // source: https://gist.github.com/pizzarob/6c9efc583a17c2643505e7d70ffb1e0e
            let xhr = new XMLHttpRequest();
            xhr.withCredentials = !!api.config.withCredentials;
            xhr.open("DELETE", new URL(server.replace(/\s+$/, "") + "/" + uniqueFieldId, api.config.baseURL).toString());
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
          }
        }
      : server,
    chunkUploads: true,
    chunkForce: true
  };

  return (
    <>
      <FilePond
        files={initialFiles}
        onupdatefiles={(files) => {
          if (Array.isArray(value)) {
            onChange?.(files.map((file) => file.serverId));
          } else {
            onChange?.(files[0]?.serverId ?? "");
          }
        }}
        chunkUploads={true}
        chunkForce={true}
        {...serverProps}
        {...props}
        ref={ref}
      />
    </>
  );
});
FileInput.displayName = "FileInput";

export { FileInput };
