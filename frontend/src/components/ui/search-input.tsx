"use client"

import type * as React from "react"
import { SearchIcon, XIcon } from "lucide-react"
import { cn } from "@/lib/utils"
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group"

/**
 * A search box with a leading search icon and a trailing clear ("x") button
 * that only appears once there is something to clear. Matching/normalising
 * the typed value (including stripping a leading "#") is the caller's job -
 * see lib/search.ts - this component only handles display.
 */
function SearchInput({
  value,
  onChange,
  className,
  ...props
}: Omit<React.ComponentProps<"input">, "value" | "onChange"> & {
  value: string
  onChange: (value: string) => void
}) {
  return (
    <InputGroup data-slot="search-input" className={cn("w-60", className)}>
      <InputGroupAddon>
        <SearchIcon />
      </InputGroupAddon>
      <InputGroupInput
        value={value}
        onChange={(event) => onChange(event.target.value)}
        {...props}
      />
      {value.length > 0 && (
        <InputGroupAddon align="inline-end">
          <InputGroupButton
            type="button"
            size="icon-xs"
            aria-label="Tøm søk"
            onClick={() => onChange("")}
          >
            <XIcon />
          </InputGroupButton>
        </InputGroupAddon>
      )}
    </InputGroup>
  )
}

export { SearchInput }
