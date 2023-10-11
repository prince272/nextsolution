"use client";

import { FC } from "react";
import { ClipboardIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import MarkdownPreview, { MarkdownPreviewProps } from "@uiw/react-markdown-preview";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { oneDark } from "react-syntax-highlighter/dist/esm/styles/prism";
import { getCodeString } from "rehype-rewrite";

import useClipboard from "@/lib/hooks/useClipboard";
import { cn } from "@/lib/utils";

export const ChatMarkdown: FC<MarkdownPreviewProps> = (props) => {
  return (
    <MarkdownPreview
      prefixCls="wmde-markdown-var"
      disableCopy
      linkTarget="_blank"
      components={{
        code: Code,
        pre(props) {
          const { children, className, node, ...rest } = props;
          return (
            <pre {...rest} className={cn("not-prose", className)}>
              {children}
            </pre>
          );
        }
      }}
      {...props}
    />
  );
};

const Code = (props: any) => {
  const { children, className, node, ...rest } = props;
  const match = /language-(\w+)/.exec(className || "");
  const content = (props.node && props.node.children ? getCodeString(props.node.children as any) : children[0] || "") as any;
  const [isCopied, setCopied] = useClipboard(content, {
    successDuration: 2000
  });

  return match ? (
    <div>
      <div className="relative flex items-center justify-between rounded-t-md bg-default-100 py-1 pl-4 pr-2 font-sans text-xs">
        <span>{match[1]}</span>
        <Button size="sm" variant="light" className="ml-auto flex h-7 items-center px-1" onPress={() => setCopied()} startContent={<ClipboardIcon className="h-5 w-5" />}>
          {isCopied ? "Copied" : "Copy code"}
        </Button>
      </div>
      <SyntaxHighlighter {...rest} style={oneDark} language={match[1]} PreTag="div" className="!mt-0 w-0 min-w-full overflow-x-auto !rounded-none">
        {content}
      </SyntaxHighlighter>
    </div>
  ) : (
    <code {...rest} className={className}>
      {children}
    </code>
  );
};
