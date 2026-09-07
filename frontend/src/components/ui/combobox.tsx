"use client"

import * as React from "react"
import { ChevronDownIcon } from "lucide-react"
import { cn } from "@/lib/utils"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command"

export type ComboboxItem = {
  id: string
  label: string
  /** Groups items under their own labelled section in the list - the "clearer entity separation" this component exists for. */
  group?: string
}

/**
 * A searchable dropdown built on the existing Command + Popover primitives
 * (see docs/13-frontend-designsystem.md, "Låntaker-/utstyrsvelger i skjema" -
 * this is the "ekte søkefelt-kombinasjon" noted there as a future upgrade
 * from the plain `Select`). The caller fetches data and passes it in as
 * `items`; this component only searches and groups what it's given.
 */
function Combobox({
  items,
  value,
  onChange,
  placeholder = "Velg…",
  searchPlaceholder = "Søk…",
  emptyText = "Ingen treff.",
  disabled,
  className,
}: {
  items: ComboboxItem[]
  value: string | null
  onChange: (id: string | null) => void
  placeholder?: string
  searchPlaceholder?: string
  emptyText?: string
  disabled?: boolean
  className?: string
}) {
  const [open, setOpen] = React.useState(false)
  const selected = items.find((item) => item.id === value) ?? null

  const groups = React.useMemo(() => {
    const byGroup = new Map<string | undefined, ComboboxItem[]>()
    for (const item of items) {
      const key = item.group
      const bucket = byGroup.get(key)
      if (bucket) {
        bucket.push(item)
      } else {
        byGroup.set(key, [item])
      }
    }
    return [...byGroup.entries()]
  }, [items])

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        disabled={disabled}
        role="combobox"
        aria-expanded={open}
        data-slot="combobox-trigger"
        className={cn(
          "flex h-8 w-full items-center justify-between gap-1.5 rounded-lg border border-input bg-transparent py-2 pr-2 pl-2.5 text-sm whitespace-nowrap transition-colors outline-none select-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-input/30 dark:hover:bg-input/50",
          className
        )}
      >
        <span className={cn("truncate", !selected && "text-muted-foreground")}>
          {selected ? selected.label : placeholder}
        </span>
        <ChevronDownIcon className="size-4 shrink-0 text-muted-foreground" />
      </PopoverTrigger>
      <PopoverContent
        data-slot="combobox-content"
        className="w-(--anchor-width) min-w-56 p-0"
      >
        <Command>
          <CommandInput placeholder={searchPlaceholder} />
          <CommandList>
            <CommandEmpty>{emptyText}</CommandEmpty>
            {groups.map(([group, groupItems]) => (
              <CommandGroup key={group ?? "__ungrouped"} heading={group}>
                {groupItems.map((item) => (
                  <CommandItem
                    key={item.id}
                    value={item.label}
                    data-checked={item.id === value}
                    onSelect={() => {
                      onChange(item.id === value ? null : item.id)
                      setOpen(false)
                    }}
                  >
                    {item.label}
                  </CommandItem>
                ))}
              </CommandGroup>
            ))}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}

export { Combobox }
