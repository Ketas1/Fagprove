# Frontend-designsystem

> **Status:** ferdig som referanse for det som er designet. Egne avsnitt
> markerer det som **ikke** er designet ennå, slik at det ikke blir borte i
> resten av dokumentet.

Dette dokumentet er regelsettet for hvordan grensesnittet skal se ut og
oppføre seg, hentet ut av det godkjente designet. Formålet er at enhver side
som bygges etterpå - uansett hvem som bygger den - bruker samme skrift, farger,
avstand og komponenter som det som allerede er godkjent, i stedet for at hver
side finner opp sine egne løsninger. Se [`frontend-page`-skillet](../.claude/skills)
og [`02-arkitektur.md`](./02-arkitektur.md) for hvor frontend passer inn i
resten av systemet.

## Kilde: designcanvaset

Alle skjermbilder, skjemaer og tilstander som beskrives her kommer fra et
design laget i Claude Design, publisert som en Artifact
(`Sport For Alle Dashboard`). Det er den visuelle fasiten - dette dokumentet
er en tekstlig oversettelse av det, ikke en ny kilde.

**Kjent begrensning ved overlevering:** lenken til designet er privat og eid
av én bruker. Før prosjektet eventuelt overtas av en annen IT-avdeling må
skjermbildene eksporteres som PNG/PDF (mulig direkte fra canvaset) og legges
ved som statiske filer, ellers mister neste utvikler tilgangen til fasiten.

Designet inneholder 15 skjermbilder ("artboards"), fordelt på to sider i
canvaset:

| Side i canvaset | Skjermbilder |
| --- | --- |
| Skjermer | Oversikt (dashbord), Utlån (tabell), Utlån (kanban), Utlån - detalj, Utstyr, Barn og foresatte, Ansatte, Logg |
| Skjemaer og velkomst | Velkomstskjerm (før innlogging), Nytt utlån, Registrer retur, Logg kontaktforsøk, Nytt utstyr, Registrer barn, Inviter ansatt |

## Gjeldende status i kodebasen

Implementert (2026-09-06): sidebar/header, Oversikt, Utlån (tabell + kanban +
detalj), Utstyr, Barn og foresatte, Ansatte, Logg og velkomstskjermen, pluss
skjemaene for nytt utlån, registrer retur, nytt utstyr og registrer barn -
alle koblet til de ekte endepunktene som finnes. Se
[ADR-0020](./adr/0020-server-lesing-klient-skriving.md) for hvordan sidene
henter data.

- `--primary` er satt til aksentfargen (`#1f6e45`), og fire statusfarge-tokens
  (`--status-danger/-warning/-success/-info-bg/-fg`) er lagt til i
  `globals.css`, i tråd med tabellen under "Statusfarger" lenger ned.
- `select`, `dialog`, `tabs`, `textarea`, `table`, `popover` og `command` er
  lagt til med `bunx shadcn@latest add`. **Kjent CLI-fallgruve:** den
  genererte koden pekte `cn`-importen til det ugyldige modulnavnet `"cn"` i
  stedet for `@/lib/utils` i alle sju nye filene - rettet manuelt. Sjekk denne
  importen først hvis en fremtidig `shadcn add` feiler på samme måte.
- Merkelapper (`StatusBadge`, `frontend/src/components/status-badge.tsx`) og
  de norske tekstene/fargevalgene for hver enum-verdi
  (`frontend/src/lib/status-labels.ts`) er samlet ett sted, ikke gjentatt per
  side.
- Det som i designet er markert som manglende backend-støtte, vises fortsatt
  i grensesnittet, men tagget med en «Ikke bygget ennå»-merkelapp/knapp
  (`frontend/src/components/not-built-yet.tsx`) med forklaring i en tooltip,
  fremfor å fjernes - se avsnittet under.

### Nytt oppdaget under implementasjonen

To ting kom for dagen først når sidene faktisk skulle vise ekte data, og er
verdt å kjenne til før noen bygger videre:

- **Bakgrunnsjobben for automatisk forfall er ikke bygget** (se den oppdaterte
  "Automatisk forfall"-seksjonen i [`02-arkitektur.md`](./02-arkitektur.md)).
  `GET /api/loans?status=Overdue` returnerer derfor ikke et lån før jobben
  finnes og faktisk har kjørt. Frontend regner ut det samme
  lesetidspunkt-sjekket selv (`effectiveLoanStatus` i
  `frontend/src/lib/loan-status.ts`, testet i `loan-status.test.ts`) i stedet
  for å stole på den lagrede `status`-kolonnen alene.
- **`Staff` og `Guardian` har mindre data enn designet forutsetter.** `Staff`
  har bare navn og om Auth0-kontoen er koblet - ingen e-post, rolle eller
  "sist innlogget" (rollebasert autorisasjon er ikke bygget, se
  [ADR-0019](./adr/0019-staff-auth0-mapping.md)), og `Guardian` har ingen
  relasjon-til-barn-felt ("Far"/"Mor"). Ansatte-siden viser derfor bare navn
  og tilkoblingsstatus, tagget med hvorfor resten mangler; lån-detaljsiden
  viser foresattes navn og kontaktinfo, men ikke relasjonen.

## Typografi

Skrift: **Geist**, allerede satt opp. Ikke bruk andre fonter.

| Bruk | Størrelse | Vekt | Eksempel |
| --- | --- | --- | --- |
| Sidetittel | 20px | 600 | "Utlån #SF-2231" |
| Kort-/seksjonstittel | 14-14.5px | 600 | "Utlån som krever oppfølging" |
| Merkevarenavn i sidebar | 13.5px | 600 | "Sport For Alle" |
| Navigasjonslenke, header, tabellrad (brødtekst) | 13-13.5px | 500 | Nav-lenker, `td`-innhold |
| Feltverdi (lesevisning) | 14px | 500 | Verdier i "Detaljer om utlånet" |
| Sekundær/metatekst | 12-12.5px | 400-500 | "8 år", tidsstempler |
| Tabelloverskrift | 11px, versaler, `letter-spacing: 0.03em` | 500 | `th`-innhold |
| Feltetikett i skjema | 12.5px | 500 | "Låntaker", "Tilstand ved retur" |
| Hjelpetekst under felt | 11.5px | 400 | "Genereres automatisk" |

Ingen kursiv, ingen understreking på lenker (farge alene markerer lenke).
`text-wrap: balance` på sidetitler er unødvendig her - titlene er korte.

## Farger

Alle farger er definert som `oklch()`. Ikke bruk hex i ny kode - skriv videre i
samme fargefunksjon som resten av `globals.css`.

### Nøytrale toner (allerede riktige)

`frontend/src/app/globals.css` har allerede nøyaktig de samme nøytrale tonene
som designet. Bruk de eksisterende Tailwind-klassene, ikke rå `oklch()`-verdier
i markup:

| Token (CSS-variabel) | Verdi | Tailwind-klasse | Brukes til i designet |
| --- | --- | --- | --- |
| `--background` | `oklch(1 0 0)` | `bg-background` | Sidebakgrunn |
| `--foreground` | `oklch(0.145 0 0)` | `text-foreground` | Primær tekst |
| `--border` | `oklch(0.922 0 0)` | `border` | Alle kantlinjer, tabellstreker |
| `--muted-foreground` | `oklch(0.556 0 0)` | `text-muted-foreground` | Sekundærtekst, ikoner i ro |
| `--muted` / `--secondary` | `oklch(0.97 0 0)` | `bg-muted` | Filterrad-bakgrunn, inaktiv nav |
| `--sidebar` | `oklch(0.985 0 0)` | `bg-sidebar` | Sidebar-bakgrunn |
| `--sidebar-accent` | `oklch(0.97 0 0)` | `bg-sidebar-accent` | Aktivt menyvalg |

Designet bruker i tillegg en enda lysere flate, `oklch(0.99 0 0)`, til
tabelloverskrifter og bilde-opplastingsbokser. Det finnes ikke som egen token
ennå - forskjellen fra `--muted` (0.97) er så liten at `bg-muted` kan brukes
direkte, eller en egen `--surface-subtle: oklch(0.99 0 0);` legges til hvis
forskjellen blir synlig i praksis.

### Aksentfarge (mangler - må legges til)

Merkevarens aksentfarge, brukt på logomerket, aktivt menyikon, lenker og
primærknapper, er **`#1F6E45`** (skoggrønn). Den finnes ikke i `globals.css`
ennå. Riktig sted er `--primary`, ikke shadcn sin `--accent` -
`--accent`/`--accent-foreground` har allerede en annen jobb i shadcn (svak
bakgrunn på hover/valgt tilstand i menyer og knapper), og å legge
merkevarefargen der ville endret alle de stedene ved et uhell.

```css
:root {
  --primary: #1F6E45;
  --primary-foreground: oklch(1 0 0);
}
```

Bruk deretter `bg-primary text-primary-foreground` for primærknapper, og
`text-primary` for lenker og aktivt menyikon - ikke skriv `#1F6E45` om igjen i
hver komponent.

### Statusfarger (mangler - må legges til)

Merkelappene (badges) er det viktigste virkemiddelet i grensesnittet for å
vise domenetilstand ved et blikk. Fargene finnes i designet, men ikke som
tokens ennå:

| Ny token (forslag) | Verdi (bakgrunn) | Verdi (tekst) | Betydning i domenet |
| --- | --- | --- | --- |
| `--status-danger-bg` / `-fg` | `oklch(0.577 0.22 25 / 0.1)` | `oklch(0.5 0.2 25)` | `LoanStatus.Overdue`, `BorrowerStatus.Banned`, "Ikke kontaktet" |
| `--status-warning-bg` / `-fg` | `oklch(0.65 0.15 70 / 0.15)` | `oklch(0.5 0.14 65)` | "Kontaktet N ganger", `IsUnreliable = true`, sen-retur-varsel |
| `--status-success-bg` / `-fg` | `oklch(0.5 0.13 152 / 0.12)` | `oklch(0.4 0.13 152)` | `LoanStatus.Returned`, `EquipmentStatus.Available`, aktiv låntaker |
| `--status-info-bg` / `-fg` | `oklch(0.55 0.14 250 / 0.1)` | `oklch(0.47 0.16 255)` | `LoanStatus.Active`, `EquipmentStatus.OnLoan` |
| (ingen egen token) | `oklch(0.145 0 0)` (fylt, ikke tonet) | `#fff` | `LoanStatus.Lost`, `EquipmentStatus.WrittenOff` - bevisst mørkere og fylt for å markere at dette er en endelig, alvorlig tilstand, ikke bare "et tall å følge opp" |

Merk at `--status-danger-fg` (`oklch(0.5 0.2 25)`) ligger svært nær shadcn sin
eksisterende `--destructive: oklch(0.577 0.245 27.325)` - samme rødtone,
forskjellig lysstyrke/metning. Ikke bytt de om med hverandre: `--destructive`
er reservert for destruktive handlinger (slette, avbryte permanent), mens
statusfargene beskriver domenetilstand. Å blande dem gjør det umulig å style
en destruktiv knapp inni en forfalt-rad uten at de ser like ut ved et uhell.

### Rundinger

shadcn sin eksisterende radius-skala (`--radius: 0.625rem` = 10px, med
avledede steg) treffer det meste av designet nøyaktig:

| Element i designet | Piksler | Tailwind-klasse |
| --- | --- | --- |
| Kort, tabellramme | 14px | `rounded-xl` |
| Knapp, inputfelt, søkefelt | 8px | `rounded-md` |
| Merkelapp (badge) | Helt rund | `rounded-full` |
| Modal/dialog | 16px | Ingen eksakt klasse - nærmeste steg er `rounded-2xl` (18px); bruk den fremfor en egendefinert verdi, forskjellen er ikke synlig |

## Avstand og grid

| Område | Verdi |
| --- | --- |
| Sidebar-bredde | 240px, fast |
| Header-høyde | 52px |
| Sideinnhold, ytre padding | 24px |
| Kort, indre padding | 16-18px |
| Avstand mellom KPI-kort | 16px |
| Hovedgrid (innhold + sidepanel) | `1.7fr` / `1fr`, gap 20-24px |
| Skjemafelt i to kolonner | `repeat(2, minmax(0,1fr))`, gap 16px |

Bruk `flex`/`grid` med `gap`, ikke marger mellom søsken - det er slik hele
designet allerede er bygget, og det er lettere å vedlikeholde.

## Komponenter

Hver rad viser hva shadcn-komponenten skal bygges eller utvides fra. "Legg
til" betyr `bunx shadcn@latest add <navn>`.

| Komponent i designet | shadcn-grunnlag | Merknad |
| --- | --- | --- |
| Sidebar med ikoner og aktiv tilstand | `Sidebar` (lagt til) | `frontend/src/components/dashboard-nav.tsx` - aktivt ikon/tekst bruker `text-primary` |
| Header (tittel, e-post, rollemerke) | Håndbygd `<header>` (ferdig) | Rollemerke viser en statisk "Ansatt"-tekst for alle, se gap-avsnittet under |
| Kort (KPI, seksjon, sidepanel) | `Card` (finnes) | Standard padding og `rounded-xl`, se over |
| Tabell med paginering | `Table` (lagt til) | Paginering er **ikke** bygget - alle rader vises samtidig. Grei nok mengde data i denne størrelsen, men et reelt hull hvis listene vokser mye |
| Statusmerkelapp | `StatusBadge` (`frontend/src/components/status-badge.tsx`) på `Badge` | Norsk tekst + tone slås opp i `frontend/src/lib/status-labels.ts`, ett sted for alle sider |
| Primærknapp / sekundærknapp | `Button` (finnes) | `variant="default"` for primær (bruker `--primary` etter fiksen over), `variant="outline"` for sekundær. **Husk:** når en `Button` rendres som `<a>` (innlogging, "Se alle"-lenker), sett `nativeButton={false}` eksplisitt - se den kjente fallgruven i [ADR-0016](./adr/0016-shadcn-ui.md) |
| Filterfaner / visningsbytte (Tabell/Kanban) | `Tabs` (lagt til) | Den lyse pillen med hvit aktiv-bakgrunn i designet er standard `TabsList`/`TabsTrigger`-utseende |
| Søkefelt | `SearchInput` (`frontend/src/components/ui/search-input.tsx`), på `InputGroup` | Filtrerer i minnet på allerede hentet data, ikke et nytt API-kall per tastetrykk. Har en "x"-knapp som bare vises når feltet ikke er tomt. Selve matchingen normaliserer søketeksten via `lib/search.ts` (`normalizeSearchQuery`), som blant annet fjerner en innledende `#` slik at et kopiert id/serienummer kan søkes opp med eller uten `#`-tegnet |
| Nedtrekksfelt (tilstand, rolle) | `Select` (lagt til) | Beholdt for korte, faste enum-lister (tilstand ved retur/nytt utstyr) der et søkefelt ikke gir noen verdi |
| Fritekstfelt (kontaktforsøk-resultat) | `Textarea` (lagt til) | Brukt i skjemaet for å logge et kontaktforsøk manuelt, se `LoanContactAttemptsSection` |
| Låntaker-/utstyrs-/kategorivelger i skjema | `Combobox` (`frontend/src/components/ui/combobox.tsx`), på `Command`+`Popover` | Bygget 2026-09-07 som den ekte søkefelt-kombinasjonen designet viser - søkefelt øverst i lista, og seksjonsoverskrift per gruppe (`group`-feltet på hvert element) for tydeligere skille. Koblet inn overalt der det tidligere var en entitetsliste i en `Select`: "Registrer barn" (velg eksisterende foresatt, togglet mot "Ny foresatt" via `Tabs`), "Nytt utlån" (låntaker og utstyr - utstyr gruppert per kategori), og kategorivalget i "Nytt utstyr"/"Rediger utstyr". `Select` selv brukes ikke lenger til noen entitetsliste, kun til korte faste enum-verdier (se raden over) |
| "..."-handlingsmeny på tabellrader | `DropdownMenu` (`frontend/src/components/ui/dropdown-menu.tsx`, ny - `@base-ui/react` har ingen ferdig `DropdownMenu` slik Radix har, så denne pakker `Menu`-primitiven i samme stil som `popover.tsx`), brukt av `RowActionsMenu` (`frontend/src/components/row-actions-menu.tsx`) | Tilbyr "Åpne" (lenke til full visning) og "Rediger" (kun når `onEdit` er gitt). **Ingen arkiver-handling** - ingen entitet støtter det ennå, se `05-api.md`. Koblet inn i alle tre tabeller nå (lån, utstyr, barn/foresatt). Samme mønster gjenbrukes for kategoriradenes "..."-meny i `category-tree.tsx` (tre alltid-synlige ikonknapper konsolidert til én), selv om den bruker `DropdownMenuItem` direkte i stedet for `RowActionsMenu` siden handlingene der er kategorispesifikke |
| Datovelger (fødselsdato o.l.) | `DatePicker` (`frontend/src/components/ui/date-picker.tsx`), på `Calendar` (`react-day-picker`, ny avhengighet - se `12-lisenser-og-vilkar.md`) i `Popover` | Erstatter nettleserens innebygde `<input type="date">`, hvis år-navigasjon er treg (bla én måned av gangen). Bruker `captionLayout="dropdown"` slik at år/måned velges direkte. Samme streng-kontrakt (`"yyyy-MM-dd"`) som det innebygde feltet. Koblet inn i "Registrer barn" (2026-09-07) - dette er den faktiske fiksen for den trege år-blaingen utvikleren meldte inn |
| Avkrysningsboks | `Checkbox` (`frontend/src/components/ui/checkbox.tsx`, ny - pakker `@base-ui/react/checkbox` i samme stil som `tabs.tsx`) | Bygget 2026-09-07 for "Identitet bekreftet"-feltet i "Registrer barn" (kun ved ny foresatt) - se `09-lover-og-regler.md` |
| Modal (Nytt utlån, Registrer retur, osv.) | `Dialog` (lagt til) | Header/body/footer-strukturen i designet stemmer med `DialogHeader`/`DialogContent`/`DialogFooter` |
| Blokkert-varsel, sen-retur-varsel | Egen liten komponent (ikke shadcn `Alert`) | Bygget inline i `new-loan-dialog.tsx`/`register-return-dialog.tsx` med statusfargene. Teksten kommer direkte fra API-ets `ProblemDetails.detail` (se `docs/05-api.md`), ikke hardkodet i frontend |
| Steg-indikator (Aktiv → Forfalt → Levert) | **Ikke bygget - forenklet til `StatusBadge`** | Lån-detaljsiden viser bare statusmerkelappen, ikke den visuelle stegvisningen fra designet. Verdt å bygge som egen komponent senere hvis stegvisningen vurderes viktig nok til å forsvare arbeidet |
| Bilde-opplasting (før/ved utlevering og retur) | Egen komponent, kun visuell | Vises som en stiplet boks tagget "Ikke bygget ennå" - se GDPR-kravet i `09-lover-og-regler.md` om at det er utstyret, aldri barnet, som skal fotograferes, når opplasting faktisk bygges |
| Kommunikasjonslogg / hendelseslinje | `LoanContactAttemptsSection` (`frontend/src/components/loans/loan-contact-attempts-section.tsx`) | Bygget 2026-09-07 - liste over kontaktforsøk (metode, resultat, tidspunkt) og et skjema (metode-`Select` + resultat-`Textarea`) for å logge et nytt, koblet inn på lån-detaljsiden. Erstatter det som tidligere var en "Ikke bygget ennå"-plassholder. Samme komponent har også "Send oppfølgings-e-post" (kun synlig når lånet er forfalt), som sender via EmailJS og logger et kontaktforsøk automatisk - se [ADR-0022](./adr/0022-emailjs-server-side.md) |
| Utestengelse (ban) på låntaker-detaljsiden | `BorrowerBanSection` (`frontend/src/components/borrowers/borrower-ban-section.tsx`) | Bygget 2026-09-07 - viser årsak og gebyrstatus når utestengt, med knapper for "Registrer gebyr betalt" og "Opphev utestengelse" (sistnevnte deaktivert til gebyret er betalt, se forretningsregel 8). Når aktiv: en "Utesteng låntaker"-knapp åpner en liten dialog med årsak-`Textarea` |
| Notater på låntaker-detaljsiden | `BorrowerNotesSection` (`frontend/src/components/borrowers/borrower-notes-section.tsx`) | Bygget 2026-09-07 - liste over notater (nyeste først) og et enkelt skjema for å legge til et nytt. Plassholderteksten i skjemaet peker til "saklig og faktabasert"-retningslinjen i `09-lover-og-regler.md` |
| Bekreft tap/skade | `MarkLostButton` (`frontend/src/components/loans/mark-lost-button.tsx`) | Bygget 2026-09-07 - egen bekreftelsesdialog før kallet gjøres, siden overgangen er terminal og ikke kan angres (setter lånet `Lost` og utstyret `WrittenOff`) |

### Nedtrekksfelt (`Select`): plassering under trigger, ikke ved valgt element

`@base-ui/react`s `Select` støtter to plasseringsmåter for popup-en:
forankret til det *valgte elementet* (`alignItemWithTrigger`, standard `true`
i biblioteket - samme idé som Radix' `position="item-aligned"`), eller alltid
rett under selve trigger-feltet (`alignItemWithTrigger={false}`, Radix'
`position="popper"`). Den første kan åpne popup-en overlappende eller over
trigger-feltet avhengig av hvilket element som er valgt - det er trolig det
utvikleren opplevde som "feil plassering" på Tilstand-/Kategori-feltene i
"Nytt utstyr". `frontend/src/components/ui/select.tsx` setter nå
`alignItemWithTrigger={false}` som standard, slik at alle bruksstedene i appen
får forutsigbar plassering rett under feltet. **Ikke bekreftet visuelt** - det
finnes ikke noe verktøy for å drive en ekte nettleser i dette miljøet ennå (se
`run`-skillet); årsaken er bekreftet ved å lese `@base-ui/react`s og denne
kodens egen kilde, ikke ved skjermbilde. Popup-en portalerer allerede korrekt
til `document.body` via `SelectPrimitive.Portal`, så den satt fast inni
dialogens transformerte undertre var **ikke** årsaken - den hypotesen ble
undersøkt og forkastet.

### Tabellkolonner: fast bredde

Både låne-, utstyrs- og barn/foresatt-tabellen (`loans-explorer.tsx`,
`equipment-explorer.tsx`, `borrowers-explorer.tsx`) bruker `table-fixed` med
en eksplisitt bredde (`w-[…%]`) per `TableHead`, i stedet for nettleserens
standard `table-layout: auto`. Uten det regner nettleseren bredden ut fra
innholdet som er synlig akkurat nå - når statusfilteret byttes (og dermed
hvilken `StatusBadge`-tekst som vises), regnes alle kolonnene ut på nytt og
tabellen "hopper" synlig. Navnekolonner har `truncate` (og `max-w-0` på selve
cellen - nødvendig fordi en tabellcelle med fast layout ellers ikke lar
underliggende blokkelementer krympe under egen innholdsbredde) slik at et
langt navn kuttes med "…" i stedet for å presse resten av raden ut av
bredden.

### Feilsider og innloggingsfeil

Bygget 2026-09-07, tre stykker, samme visuelle stil som forsiden
(`app/page.tsx`):

- **`app/not-found.tsx`** - Next.js' `not-found`-konvensjon (bekreftet mot
  `node_modules/next/dist/docs`). Fanger både eksplisitte `notFound()`-kall
  fra en side og enhver URL som ikke matcher noen rute. Lenken går til
  `/dashboard` hvis innlogget, ellers `/` - sjekket med `auth0.getSession()`.
- **`app/error.tsx`** - React-feilgrense for uventede kjøretidsfeil.
  `retry`-propen er denne Next.js-versjonens navn (stabilt fra 16.3.0, het
  `reset` før) - bekreftet mot dokumentasjonen i stedet for antatt, siden
  `frontend/AGENTS.md` advarer om at slikt kan avvike fra treningsdata.
- **Innloggingsfeil** - undersøkt i selve SDK-koden
  (`node_modules/@auth0/nextjs-auth0/dist/server/auth-client.js`), ikke
  antatt: SDK-ets egen standard-`onCallback` returnerer ren tekst
  (`new NextResponse(error.message, { status: 500 })`) direkte fra
  callback-ruten ved en mislykket innlogging (avvist samtykke, feilkonfigurert
  Auth0-tenant, feil i token-utveksling) - dette skjer *før* noe React
  rendres, så verken `error.tsx` eller `not-found.tsx` fanger det. Fikset ved
  å gi `Auth0Client` (`lib/auth0.ts`) en egen `onCallback` som beholder
  standardoppførselen ved suksess, men omdirigerer til `/?authError=<melding>`
  ved feil, slik at `app/page.tsx` kan vise den i appens egen stil i stedet
  for en rå tekstrespons.

## Ikonografi

Designet bruker håndtegnede SVG-er fordi Design Components-formatet ikke kan
importere npm-pakker. **I den faktiske frontend-koden skal ikonene komme fra
`lucide-react`** (allerede en avhengighet, se
[ADR-0016](./adr/0016-shadcn-ui.md)), ikke som kopierte inline-SVG-er. Stilen
er den samme uansett - strekbasert, 24×24, rund linjeavslutning - så det
riktige lucide-ikonet for hver plass i designet (hus for Oversikt, boks for
Utlån, sko for Utstyr, personer for Barn og foresatte, skjold for Ansatte,
dokument for Logg, osv.) skal se ut som en direkte erstatning, ikke en
tilnærming.

Bruk aldri emoji som ikon eller markør noe sted i grensesnittet.

## Utstyrskategorier: kategoritre (ikke i det opprinnelige designet)

Kategorihierarkiet (`components/equipment/category-tree.tsx`) ble lagt til
etter at det opprinnelige designcanvaset var laget, og finnes derfor ikke der
- se [ADR-0021](./adr/0021-hierarkiske-utstyrskategorier.md) for hvorfor og
hvilke alternativer som ble vurdert. Mønsteret er bevisst enkelt:

- Ett venstre panel (`CategoryTree`) med et sammenleggbart tre - ingen egen
  rute, ingen brødsmulesti. Valgt kategori er lokal komponenttilstand
  (`selectedCategoryId` i `EquipmentWorkspace`).
- Høyre side er den samme utstyrstabellen som før (`EquipmentExplorer`),
  bare filtrert på valgt kategori. "Alt utstyr" (ingen kategori valgt) er
  nøyaktig den gamle, flate visningen.
- En kategori kan ha både underkategorier og eget utstyr samtidig - treet
  viser det ene, tabellen det andre, side om side.
- Ny/gi nytt navn/slett er små ikonknapper per rad i treet, med samme
  `Dialog`-mønster som resten av appen. Sletting viser backend sin `409`
  (`CategoryHasSubcategories`/`CategoryHasEquipment`) direkte som feiltekst i
  dialogen, akkurat som det blokkerte utlånet.
- Treets dybde er ikke begrenset i grensesnittet heller - det følger av at
  det ikke er begrenset i domenet (se ADR-0021), ikke en egen frontend-regel.

## Tilstander som ikke er designet ennå

Disse manglet allerede da designet ble gjennomgått, og er ikke lagt til
siden - de er reelle hull, ikke noe denne dokumentasjonen løser stilltiende:

- **Lasting.** Ingen skjelett-/spinner-tilstand er designet. `Skeleton` finnes
  allerede i `components/ui/`; bruk den som utgangspunkt for tabellrader og
  kort mens data hentes, i tråd med resten av paletten, fremfor å finne opp en
  ny visuell løsning.
- **Tomme lister.** Ingen "ingen resultater"-tilstand er designet (for
  eksempel et søk uten treff, eller en butikk uten forfalte lån).
- **Feilmeldinger i grensesnittet**, inkludert nøyaktig hvordan en `409
  Conflict` fra det blokkerte utlånet vises der brukeren faktisk trykker
  "Registrer utlån" (skjemamodalen viser tilstanden som en veksle i designet,
  men ikke hvordan feilen fra et ekte API-kall skal se ut første gang den
  oppstår).
- **Låntaker-detaljside.** Bygget 2026-09-07 (`app/dashboard/borrowers/[id]/page.tsx`)
  - viser barnets detaljer og foresattes kontaktinformasjon. Fortsatt **ikke**
  bygget: utesteng-/opphev utestengelse-dialog, notatvisning, eller
  lånehistorikk for barnet - selve utestengelsesregelen er en sentral
  forretningsregel (se `03-domenemodell.md`) og API-endepunktene for den finnes
  allerede (`05-api.md`), men den fulle oppfølgingsarbeidsflyten i
  grensesnittet er større enn denne detaljsiden og er ikke del av denne
  omgangen.
- **Mobil/responsivt.** Alle 15 skjermbilder er tegnet på fast bredde
  (1440px). Ingenting er sagt om hvordan sidebar, tabeller eller modaler
  oppfører seg på et smalere vindu.
- **Mørk modus.** `globals.css` har allerede et fullstendig `.dark`-sett med
  tokens (arvet fra shadcn sin standardoppsett), men designet er kun laget i
  lys modus, og ingen har bedt om mørk modus for dette produktet.

## Tilgjengelighet

- Alle ikoner er dekorative når teksten ved siden av allerede sier det samme
  (f.eks. "Utlån" + huskeikon) - sett `aria-hidden="true"` på selve SVG-en i de
  tilfellene, ikke en alt-tekst som gjentar menyteksten.
- Kontrast er ikke sjekket ennå. Spesielt `--status-warning-fg` på
  `--status-warning-bg` (gul/oransje på lys bakgrunn) bør kontrollmåles mot
  WCAG AA i implementasjonen, siden det er den svakeste fargekombinasjonen i
  paletten.
- Fokusring på alle interaktive elementer (lenker, knapper, feltrader i
  tabellen som lenker til detaljside) - designet viser ikke en fokus-tilstand,
  siden det er statiske mockups. Bruk shadcn sin innebygde `focus-visible`-ring
  fremfor å fjerne den.
- Treffflate på klikkbare rader og knapper: minimum 44px høyde på reelt
  klikkbare elementer på mindre skjermer, selv der designet viser en visuelt
  slankere rad (13-16px tekst i en 40px-rad er greit; en knapp på 34px høyde
  slik enkelte sekundærknapper er tegnet, er i minste laget og bør økes ved
  touch-bruk hvis produktet noen gang skal driftes på nettbrett i butikken).

## Videre lesning

- Domenemodell og tilstandsmaskiner (det badge-fargene refererer til):
  [`03-domenemodell.md`](./03-domenemodell.md)
- Hvorfor shadcn/ui er eneste komponentbibliotek:
  [ADR-0016](./adr/0016-shadcn-ui.md)
- Personvernkravet bak "fotografer utstyret, aldri barnet":
  [`09-lover-og-regler.md`](./09-lover-og-regler.md)
