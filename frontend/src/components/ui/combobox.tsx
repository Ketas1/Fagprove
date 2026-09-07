"use client"

import * as React from "react"
import { ChevronDownIcon, PlusIcon } from "lucide-react"
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
  onCreateNew,
  createLabel = (query) => `Opprett «${query}»`,
}: {
  items: ComboboxItem[]
  value: string | null
  onChange: (id: string | null) => void
  placeholder?: string
  searchPlaceholder?: string
  emptyText?: string
  disabled?: boolean
  className?: string
  /**
   * When given, shows a "create new" row once the typed text has no exact
   * match among `items` - the caller does the actual creating (the API
   * call, merging the new item into `items`, and calling `onChange` to
   * select it). This component only offers the shortcut, it has no opinion
   * on what "create" means for a given entity.
   */
  onCreateNew?: (query: string) => void
  /** Label for the create-new row. Defaults to `Opprett «query»`. */
  createLabel?: (query: string) => string
}) {
  const [open, setOpen] = React.useState(false)
  const [query, setQuery] = React.useState("")
  const selected = items.find((item) => item.id === value) ?? null
  const trimmedQuery = query.trim()
  const lowerQuery = trimmedQuery.toLowerCase()
  const hasExactMatch = items.some((item) => item.label.toLowerCase() === lowerQuery)
  const showCreateRow = Boolean(onCreateNew) && trimmedQuery.length > 0 && !hasExactMatch

  // Filtering is done here, not left to cmdk's own built-in fuzzy filter
  // (disabled below via `shouldFilter={false}`) - the create-new row's
  // "value" doesn't correspond to any real item, and mixing a synthetic
  // value into cmdk's own scoring made the row's visibility unreliable.
  // Doing the filtering ourselves keeps it simple, predictable, and in one
  // place instead of two. No manual `useMemo` here - the React Compiler
  // handles memoizing this automatically, and fought a hand-written one.
  const filteredItems = lowerQuery
    ? items.filter((item) => item.label.toLowerCase().includes(lowerQuery))
    : items

  const groups: [string | undefined, ComboboxItem[]][] = []
  for (const item of filteredItems) {
    const bucket = groups.find(([key]) => key === item.group)
    if (bucket) {
      bucket[1].push(item)
    } else {
      groups.push([item.group, [item]])
    }
  }

  return (
    <Popover
      open={open}
      onOpenChange={(next) => {
        setOpen(next)
        if (!next) setQuery("")
      }}
    >
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
        <Command shouldFilter={false}>
          <CommandInput placeholder={searchPlaceholder} value={query} onValueChange={setQuery} />
          <CommandList>
            {filteredItems.length === 0 && !showCreateRow && <CommandEmpty>{emptyText}</CommandEmpty>}
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
            {showCreateRow && (
              <CommandGroup>
                <CommandItem
                  value={`__create__${trimmedQuery}`}
                  onSelect={() => {
                    onCreateNew?.(trimmedQuery)
                    setOpen(false)
                  }}
                >
                  <PlusIcon />
                  {createLabel(trimmedQuery)}
                </CommandItem>
              </CommandGroup>
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}

export { Combobox }
