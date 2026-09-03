# ADR-0016: shadcn/ui som eneste komponentbibliotek i frontend

- **Status:** Akseptert
- **Dato:** 2026-09-03

## Kontekst

Frontend gikk fra ett enkelt statussideutkast til å trenge et innloggingsbilde
og et dashbord i samme endring. Utviklingsperioden er kort, og
Tailwind-klasser skrevet for hånd for hver knapp, hvert kort og hver
statusindikator er tid som ikke går til noe brukeren faktisk ser forskjell på.

## Beslutning

Frontend bruker **shadcn/ui** for alle UI-komponenter, ikke bare der det er
bekvemt. shadcn er ikke et npm-pakke i vanlig forstand - CLI-en
(`bunx shadcn@latest add <komponent>`) kopierer komponentkildekoden inn i
`src/components/ui/`, og koden eies av prosjektet fra det øyeblikket den er
lagt til, på samme måte som all annen kode i repoet.

Oppsettet bruker standardvalgene fra `shadcn init`: stilen `base-nova`, Base
UI (`@base-ui/react`) som det tilgjengelige, ustylede fundamentet under
komponentene, og `lucide-react` for ikoner.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| Håndskrevne Tailwind-komponenter (fortsette som før) | Ingen ny avhengighet. Full kontroll over hver piksel. | Hver knapp, hvert kort og hver statusmarkering skrives og vedlikeholdes fra bunnen. Tar tid fra en kort utviklingsperiode uten å gi noe prosjektet trenger. | Tiden forsvares ikke av gevinsten |
| Et ferdig komponentbibliotek som importeres (for eksempel MUI eller Chakra) | Raskt i gang, mye dokumentasjon. | Egen designspråk og temasystem å tilpasse. Biblioteket er en ekstern avhengighet som må oppdateres og kan gjøre brytende endringer. | Løser det samme problemet som shadcn, men med mindre kontroll over resultatet |
| **shadcn/ui** | Komponentkilden kopieres inn og eies av prosjektet - ingen versjonslåsing eller brytende oppgraderinger utenfra. Tilgjengelige, ferdig testede komponenter (fra Base UI) under en konsistent, moderne stil. | Underliggende avhengigheter (`@base-ui/react`, `class-variance-authority`, `lucide-react` med flere) må likevel lisenssjekkes, se `12-lisenser-og-vilkar.md`. Komponenter som legges til senere må hentes eksplisitt med CLI-en, ikke bare importeres. | Valgt |

## Konsekvenser

**Positivt**

- Innloggingsbildet, dashbordet og statusvisningen bruker samme
  knapp-, kort- og merkelapp-komponenter, uten at noen av dem er
  håndskrevet fra bunnen.
- Komponentkildekoden ligger i repoet og kan endres direkte når noe må
  tilpasses, uten å vente på eller patche et eksternt bibliotek.
- Alle underliggende avhengigheter er permissivt lisensiert (MIT, Apache 2.0,
  ISC), se `12-lisenser-og-vilkar.md`.

**Negativt eller risiko**

- "Kun shadcn" er en disiplinregel, ikke noe verktøyet håndhever - en
  fremtidig endring som legger til en håndskrevet komponent ved siden av vil
  ikke bli stoppet automatisk.
- Prosjektet er nå på "base-nova"-stilen og Base UI som fundament. Å bytte til
  et annet base-bibliotek (Radix, Base UI sin forgjenger, eller React Aria)
  senere krever å kjøre `shadcn init` på nytt og migrere komponentene som
  allerede er lagt til.
- **Base UI-spesifikk fallgruve:** `Button` antar som standard at elementet
  den erstatter via `render` er et ekte `<button>`. Rendres den som noe annet
  (for eksempel en `<a>`, som i innloggings- og utloggingslenkene) uten å
  sette `nativeButton={false}` eksplisitt, advarer Base UI i konsollen om at
  native button-semantikk går tapt. Sett alltid `nativeButton={false}` når
  `render` peker på noe annet enn en `<button>`.
