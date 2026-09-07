"use client"

import * as React from "react"
import { CalendarIcon } from "lucide-react"
import { cn } from "@/lib/utils"
import { dateToIsoDate, isoDateToDate } from "@/lib/date"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Calendar } from "@/components/ui/calendar"

/**
 * Replaces the browser's native `<input type="date">`, whose year navigation
 * is a slow one-month-at-a-time scroll. Uses `captionLayout="dropdown"` so
 * jumping to a specific year/month is a single select, not many clicks.
 * Value/onChange use the same "yyyy-MM-dd" string contract as the native
 * input it replaces, so swapping one for the other is a drop-in change.
 */
function DatePicker({
  value,
  onChange,
  placeholder = "Velg dato",
  disabled,
  fromYear,
  toYear,
  id,
  className,
}: {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  disabled?: boolean
  /** Earliest selectable year - defaults to 100 years before `toYear`. */
  fromYear?: number
  /** Latest selectable year - defaults to the current year. */
  toYear?: number
  id?: string
  className?: string
}) {
  const [open, setOpen] = React.useState(false)
  const selected = isoDateToDate(value)
  const resolvedToYear = toYear ?? new Date().getFullYear()
  const resolvedFromYear = fromYear ?? resolvedToYear - 100

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        id={id}
        disabled={disabled}
        data-slot="date-picker-trigger"
        className={cn(
          "flex h-9 w-full items-center gap-2 rounded-md border border-input bg-transparent px-3 text-sm outline-none transition-colors focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50",
          className
        )}
      >
        <CalendarIcon className="size-4 shrink-0 text-muted-foreground" />
        <span className={cn(!selected && "text-muted-foreground")}>
          {selected ? selected.toLocaleDateString("nb-NO") : placeholder}
        </span>
      </PopoverTrigger>
      <PopoverContent align="start" className="w-auto p-0">
        <Calendar
          mode="single"
          captionLayout="dropdown"
          startMonth={new Date(resolvedFromYear, 0)}
          endMonth={new Date(resolvedToYear, 11)}
          selected={selected ?? undefined}
          defaultMonth={selected ?? new Date(resolvedToYear, 0)}
          disabled={{ after: new Date(resolvedToYear, 11, 31) }}
          onSelect={(date) => {
            if (date) {
              onChange(dateToIsoDate(date))
              setOpen(false)
            }
          }}
        />
      </PopoverContent>
    </Popover>
  )
}

export { DatePicker }
