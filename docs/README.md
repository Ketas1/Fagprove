# Dokumentasjon - Sport For Alle

Dette er dokumentasjonen for løsningen. Den er en del av leveransen, ikke et
vedlegg som skrives til slutt.

## Dokumentasjonsstrategi

Dokumentasjonen skal dekke tekniske valg, arkitekturdesign, databasedesign,
backend/API, testing, sikkerhet, relevante lover og regler, lisenser, og bruk av
systemet. Strategien under er laget for å oppfylle dette uten at dokumentasjonen
blir en egen sluttspurt.

### Hva dokumentasjonen skal oppnå

Løsningen er et **forslag til en løsning**, ikke et ferdig produksjonssystem.
Men den skal kunne **overtas av en annen IT-avdeling**, som skal kunne
videreutvikle den og komme til et driftsklart nivå uten mye ekstra arbeid.
Avstanden fra forslag til drift skal derfor være synlig og beskrevet - ikke
skjult.

Oppdragsgiver er en kommunalt eid virksomhet som behandler personopplysninger om
barn. Lover og regler, personvern, pakkelisenser og vilkår for eksterne tjenester
er derfor en forventet del av dokumentasjonen, ikke et vedlegg.

**Fem prinsipper:**

1. **Dokumentasjon skrives samtidig med koden.** Et dokument oppdateres i samme
   endring som koden det beskriver. Dokumentasjon som utsettes til slutten blir
   enten feil eller blir ikke skrevet.
2. **Tekniske valg skrives som ADR-er.** Hvert teknologivalg og hvert større
   designvalg får et eget kort dokument i [`adr/`](./adr/) som forklarer hva som
   ble valgt, hvilke alternativer som ble vurdert, og hvorfor. Da er
   begrunnelsen dokumentert der og da, ikke rekonstruert i etterkant.
3. **Forklar hvorfor, ikke bare hva.** Koden viser allerede hva som skjer.
   Dokumentasjonen skal svare på hvorfor det er løst slik, og hva som ble vraket.
4. **Skrevet for en utvikler som ikke har sett koden.** Et av målene med
   løsningen er at den skal være enkel for en annen utvikler å forstå, drifte og
   videreutvikle. Dokumentasjonen er der det målet innfris.
5. **Diagrammer som Mermaid i Markdown.** Da versjoneres diagrammene sammen med
   koden og kan endres i en pull request, i stedet for å ligge som bilder som
   raskt blir utdaterte.

**Språk:** dokumentasjonen er på norsk. Koden, API-et og AI-instruksene er på
engelsk. Se [ADR-0010](./adr/0010-domenespraak-engelsk-i-kode.md) for
begrunnelsen, og ordlisten i [`03-domenemodell.md`](./03-domenemodell.md) for
oversettelsene.

## Innhold

| Dokument | Innhold | Status |
| --- | --- | --- |
| [01-losningsbeskrivelse.md](./01-losningsbeskrivelse.md) | Problemene, målene, aktørene og hvordan systemet løser dem | Ferdig |
| [02-arkitektur.md](./02-arkitektur.md) | Systemarkitektur, lagdeling og dataflyt | Ferdig |
| [03-domenemodell.md](./03-domenemodell.md) | Domenemodell, tilstandsmaskiner, forretningsregler og ordliste | Ferdig |
| [04-databasedesign.md](./04-databasedesign.md) | Databaseskjema, tabeller, relasjoner og migrasjoner | Påbegynt |
| [05-api.md](./05-api.md) | Backend-API, endepunkter, feilhåndtering | Påbegynt |
| [06-autentisering.md](./06-autentisering.md) | Authentication og authorization med Auth0 | Påbegynt |
| [07-testing.md](./07-testing.md) | Teststrategi, testnivåer og hva som testes hvor | Påbegynt |
| [08-sikkerhet.md](./08-sikkerhet.md) | Sikkerhetstiltak og trusselvurdering | Påbegynt |
| [09-lover-og-regler.md](./09-lover-og-regler.md) | GDPR og personvern, med vekt på data om barn | Ferdig |
| [10-bruk-av-systemet.md](./10-bruk-av-systemet.md) | Brukerveiledning for ansatte | Ikke påbegynt |
| [11-utviklingsmiljo.md](./11-utviklingsmiljo.md) | Prosjektoppsett, teknologistack, filstruktur, Docker og CI | Ferdig |
| [12-lisenser-og-vilkar.md](./12-lisenser-og-vilkar.md) | Pakkelisenser og vilkår for eksterne tjenester | Påbegynt |
| [13-frontend-designsystem.md](./13-frontend-designsystem.md) | Typografi, farger, komponenter og tilstander - regelsettet bak alle sider | Ferdig |
| [adr/](./adr/) | Architecture Decision Records - alle tekniske valg | Løpende |
| [arbeidslogg.md](./arbeidslogg.md) | Daglig arbeidslogg - grunnlag for sluttrapporten | Løpende |

Dokumenter som ikke er ferdige finnes allerede med struktur og overskrifter,
slik at de fylles ut når arbeidet gjøres.

## Hvordan dokumentasjonen holdes oppdatert

- Når en teknisk beslutning tas: opprett en ADR (`/adr` i Claude Code).
- Når kode endres: oppdater dokumentet som beskriver den delen i samme commit.
- Når en ny pakke legges til: før den inn i
  [`12-lisenser-og-vilkar.md`](./12-lisenser-og-vilkar.md) med lisens.
- Når et nytt dokument legges til: legg det inn i tabellen over.
- Når et domenebegrep innføres: legg det i ordlisten i `03-domenemodell.md` og i
  glossaret i `CLAUDE.md`, slik at norsk og engelsk holdes synkronisert.
