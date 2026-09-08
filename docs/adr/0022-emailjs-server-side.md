# ADR-0022: Oppfølgings-e-post sendes fra backend, ikke nettleseren

- **Status:** Akseptert
- **Dato:** 2026-09-07

## Kontekst

Ansatte skal kunne trigge en manuell oppfølgings-e-post til foresatt for et
forfalt lån - se forretningsregel 6 i `03-domenemodell.md`. Løsningen er
EmailJS, en tredjeparts e-posttjeneste utvikleren allerede har konto hos.

EmailJS støtter to bruksmåter: en nettleser-SDK (`@emailjs/browser`) som
kaller EmailJS direkte fra klientkoden, eller et rent REST-API som kan kalles
fra hvilken som helst server. Spørsmålet er hvilken av disse dette systemet
skal bruke, gitt at ADR-0015 allerede har bestemt at nettleseren aldri kaller
noe annet enn appens egen samme-opprinnelse-proxy.

## Beslutning

Backend kaller EmailJS sitt REST-API (`POST
https://api.emailjs.com/api/v1.0/email/send`) direkte via `HttpClient`, som
en vanlig server-til-server-forespørsel. Ingen ny npm-pakke er lagt til i
frontend - dette er ett rent JSON-kall, ikke noe som trenger et SDK.

`LoanService.SendFollowUpEmailAsync` er det eneste stedet i domenelaget som
kjenner til at e-posten sendes via EmailJS; selve HTTP-kallet er isolert i
`Services/FollowUpEmailSender.cs`, som bare snakker med EmailJS og ikke vet
noe om lån, låntakere eller foresatte.

## Alternativer som ble vurdert

| Alternativ | Fordeler | Ulemper | Hvorfor ikke valgt |
| --- | --- | --- | --- |
| `@emailjs/browser` fra en klientkomponent | EmailJS sitt eget hovedscenario, minst kode å skrive | Bryter ADR-0015: nettleseren ville kalt en ekstern tjeneste direkte, ikke bare appens egen proxy. Service-/malid-er havner i klientbunten. Gir ingen reell fordel her - EmailJS sitt REST-API er like enkelt å kalle fra .NET | Motsier et allerede akseptert arkitekturvalg uten god nok grunn |
| **Backend kaller EmailJS sitt REST-API** | Holder seg til ADR-0015: nettleseren ser aldri EmailJS. Hemmelighetene (`PrivateKey` spesielt) ligger kun i backend-miljøvariabler, aldri i klientkode. Sender-handlingen kan knyttes direkte til `ContactAttempt`-loggingen i samme transaksjon | EmailJS krever at "API requests" for ikke-nettleser-kall aktiveres manuelt i kontoinnstillingene deres - et engangs oppsett utvikleren må gjøre selv, ikke noe koden kan løse | Valgt |

## Konsekvenser

**Positivt**

- Ingen unntak fra ADR-0015 - samme regel gjelder for absolutt alt av
  ekstern kommunikasjon.
- `EmailJs__PrivateKey` (EmailJS sin "Private Key"/`accessToken`) er en reell
  hemmelighet og ligger kun som backend-miljøvariabel, aldri i frontend-kode
  eller i en klientbunt.
- Sending og logging av kontaktforsøk skjer i samme handling
  (`LoanService.SendFollowUpEmailAsync`) - mislykket sending logger ingen
  `ContactAttempt`, siden den ikke faktisk fant sted.

**Negativt eller risiko**

- Ett ekstra, eksternt avhengighetspunkt i backend-et som kan feile
  (nettverk, feil konfigurert konto, EmailJS sin egen kvote på 200
  forespørsler/måned på gratisplanen). Feilen vises til ansatt som en tydelig
  feilmelding (`EmailSendFailed`), ikke en stille feil.
- Tester må aktivt hindre ekte kall til EmailJS - løst ved å registrere den
  utgående `HttpClient`-en under et navngitt klientnavn ("EmailJs") som
  testoppsettet bytter ut med en falsk handler, se
  `SportForAlle.Tests/TestSupport/AuthenticatedWebApplicationFactory.cs` og
  `FakeHttpMessageHandler.cs`. Uten dette ville `dotnet test` brukt av
  gratiskvoten for hver kjøring.
