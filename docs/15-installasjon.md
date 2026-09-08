# Installasjon - få systemet til å kjøre

Denne veiledningen får deg fra ingenting til et **kjørende system du er logget
inn i**. Den forutsetter ingen kjennskap til prosjektet, og den ber deg ikke ta
et eneste valg underveis. Følg stegene i rekkefølge.

Regn med **15-20 minutter**, det meste av det nedlasting.

> **Dette dokumentet handler bare om å få det til å kjøre.** Når du er inne:
>
> - **Hvordan systemet brukes** til daglig: [`10-bruk-av-systemet.md`](./10-bruk-av-systemet.md)
> - **Hvordan koden er bygget**, og hva som gjenstår: [`14-utviklerhandbok.md`](./14-utviklerhandbok.md)
> - **Utviklingsmiljøet** i detalj - filstruktur, migrasjoner, CI: [`11-utviklingsmiljo.md`](./11-utviklingsmiljo.md)

---

## Hva du skal ha fått utlevert

To ting, levert utenfor kodelageret:

| Fil | Innhold |
| --- | --- |
| `.env` | Ferdig utfylt med alle verdier - database, Auth0 og EmailJS |
| `BRUKERE.md` | Brukernavn og passord til testbrukerne du logger inn med |

Har du ikke fått disse, stopp her og be om dem. **Systemet kan ikke starte uten
`.env`-filen**, og du kan ikke logge inn uten en testbruker.

Du trenger **ingen egen Auth0-konto og ingen egen EmailJS-konto**. Verdiene du
har fått peker på et ferdig oppsett, og innloggingen er allerede satt opp til å
virke mot `http://localhost:3000` - altså din egen maskin. Se
[Om de utleverte verdiene](#om-de-utleverte-verdiene) nederst.

---

## Forutsetninger

Installer disse først. Alle er gratis.

| Verktøy | Versjon | Sjekk at det virker |
| --- | --- | --- |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | nyeste | `docker --version` |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.x | `dotnet --version` |
| [Bun](https://bun.com/docs/installation) | 1.3 eller nyere | `bun --version` |
| [Git](https://git-scm.com/downloads) | nyeste | `git --version` |

**Docker Desktop må kjøre** før du begynner - ikke bare være installert. Start
det, og vent til statusen er grønn.

Kommandoene under er skrevet for én terminal om gangen. På Windows fungerer de i
PowerShell.

---

## Oppskriften

### 1. Hent koden

```bash
git clone https://github.com/Ketas1/Fagprove.git
cd Fagprove
```

### 2. Legg `.env` i rotmappen

Kopier den utleverte `.env`-filen inn i mappen `Fagprove` - altså samme sted som
`README.md` og `docker-compose.yml` ligger.

**Ikke** legg den i `backend/` eller `frontend/`. Begge lagene leser den samme
filen fra rotmappen.

Sjekk at den ligger riktig:

```bash
ls .env          # Windows PowerShell: dir .env
```

> Filen er utelatt fra Git med vilje og skal aldri sjekkes inn.

### 3. Installer EF Core-verktøyet

Dette er et eget kommandolinjeverktøy som ikke følger med .NET SDK-en. Uten det
feiler steg 5.

```bash
dotnet tool install --global dotnet-ef
```

Har du det fra før, får du beskjed om at det allerede er installert - det er
greit. En nyere versjon av verktøyet enn EF Core-versjonen i prosjektet er ikke
et problem.

Sjekk:

```bash
dotnet ef --version
```

> Får du `command not found` rett etter installasjonen, må du åpne et nytt
> terminalvindu. Verktøyet legges i `~/.dotnet/tools`, som først kommer inn i
> PATH i en ny terminal.

### 4. Start databasen

```bash
docker compose up -d db
```

Databasen kjører i Docker og blir liggende på **port 5433**. Sjekk at den er
oppe:

```bash
docker compose ps
```

Statusen skal være `running` og `healthy`. Er den ikke det, vent noen sekunder
og prøv igjen - den bruker litt tid første gang.

### 5. Opprett databaseskjemaet

Databasen er tom når containeren starter. Dette steget lager tabellene.

```bash
cd backend
dotnet ef database update --project SportForAlle.Api --startup-project SportForAlle.Api
```

Du skal se en linje per migrasjon, og til slutt `Done.`

**Dette steget er lett å hoppe over, og det er den vanligste grunnen til at
systemet ser ødelagt ut.** Uten tabellene starter API-et helt fint, men hver
eneste side feiler.

### 6. Start backend

Fortsatt i `backend`-mappen:

```bash
cd SportForAlle.Api
dotnet run
```

Vent til du ser:

```
Now listening on: http://localhost:5080
```

**La dette terminalvinduet stå åpent.** Backend kjører så lenge det gjør det.

### 7. Start frontend

Åpne et **nytt** terminalvindu, og gå til `Fagprove`-mappen:

```bash
cd frontend
bun install
bun dev
```

Første `bun install` tar litt tid. Vent til du ser:

```
- Local: http://localhost:3000
```

**La også dette vinduet stå åpent.**

Du har nå tre ting i gang samtidig: databasen i Docker, backend i ett vindu,
frontend i et annet.

### 8. Logg inn

Åpne <http://localhost:3000> i nettleseren og trykk **«Logg inn»**.

Du sendes til innloggingssiden til Auth0. Bruk brukernavnet og passordet fra
`BRUKERE.md`.

### 9. Koble ansattprofilen din

Etter innlogging havner du på oversikten, og **du får sannsynligvis siden «Noe
gikk galt»**. Det er forventet første gang, og det er ikke en feil i
installasjonen.

Grunnen: systemet krever at innloggingskontoen din er koblet til en
ansattprofil før den får se data. Det er et bevisst sikkerhetsvalg, se
[ADR-0019](./adr/0019-staff-auth0-mapping.md).

Slik kobler du:

1. Gå til <http://localhost:3000/dashboard/staff>.
2. Under **«Din ansattprofil»**: skriv navnet ditt og trykk **«Opprett profil»**.
3. Profilen dukker opp i listen under, merket «Ikke koblet». Trykk **«Koble til
   min konto»** på den raden.
4. Last siden på nytt. Det skal nå stå «Du er koblet som *navnet ditt*».

Dette gjøres **én gang per innloggingskonto**. Logger en kollega inn med en
annen bruker, må de gjøre det samme for seg.

### 10. Sjekk at alt henger sammen

Gå til <http://localhost:3000/dashboard>. Oversikten skal nå vises med fire
tellere øverst, alle på 0.

Ferdig.

---

## Verifisering

| Adresse | Skal vise |
| --- | --- |
| <http://localhost:3000> | Forsiden med «Logg inn» - eller sender deg rett til oversikten hvis du er innlogget |
| <http://localhost:3000/dashboard> | Oversikten, med fire tellere og menyen til venstre |
| <http://localhost:3000/api/health> | `{"status":"ok","database":"up"}` når du er innlogget |
| <http://localhost:5080/scalar> | API-dokumentasjonen |

Vil du i tillegg bekrefte at koden er hel, kjør testene: `cd backend &&
dotnet test` og `cd frontend && bun run test`. Begge skal være grønne.

### Databasen er tom - det er meningen

Systemet leveres uten testdata. Alle listene er tomme og rapportene viser 0
til du registrerer noe selv. Det er ikke en feil.

Vil du se systemet med innhold i, registrer i denne rekkefølgen - hvert steg
forutsetter det forrige:

**foresatt og barn** → **kategori og utstyr** → **utlån**

Framgangsmåten for hvert av dem står i
[`10-bruk-av-systemet.md`](./10-bruk-av-systemet.md).

---

## Hvis noe feiler

| Symptom | Årsak | Løsning |
| --- | --- | --- |
| `dotnet ef` gir `command not found` | Verktøyet mangler, eller PATH er ikke oppdatert | Kjør steg 3. Er det allerede kjørt, åpne et **nytt** terminalvindu |
| `password authentication failed for user "sportforalle"` | Du har en PostgreSQL installert lokalt som holder port 5432, og tilkoblingen treffer den i stedet | Databasen skal ligge på **5433**. Sjekk at `POSTGRES_PORT=5433` i `.env`, og at tilkoblingsstrengen sier `Port=5433` |
| Hver side gir «Noe gikk galt», også etter at du har koblet ansattprofilen | Steg 5 ble hoppet over - tabellene finnes ikke | Kjør `dotnet ef database update` (steg 5) og last siden på nytt |
| `Noe gikk galt` bare på oversikten, rett etter første innlogging | Kontoen er ikke koblet til en ansattprofil ennå | Gjør steg 9 |
| Innloggingen sender deg tilbake til forsiden med en feilmelding | `.env` mangler, ligger feil sted, eller er ufullstendig | Sjekk at `.env` ligger i **rotmappen**, ikke i `backend/` eller `frontend/`. Start backend og frontend på nytt etterpå - `.env` leses ved oppstart |
| `MSB3021: Unable to copy ... being used by another process` | En `dotnet run` kjører allerede og holder filen | Stopp den med `Ctrl+C` i vinduet den kjører i, og prøv igjen |
| Frontend starter, men alle sider er tomme eller uendrede | Foreldet byggcache | Stopp `bun dev`, slett `frontend/.next`, start på nytt |
| Docker-kommandoene gir `cannot connect to the Docker daemon` | Docker Desktop kjører ikke | Start Docker Desktop og vent til statusen er grønn |

Flere feller, av typen som dukker opp når du begynner å *utvikle*, står under
«Kjente feller» i [`14-utviklerhandbok.md`](./14-utviklerhandbok.md).

---

## Stoppe og starte igjen

**Stoppe:** `Ctrl+C` i begge terminalvinduene, og så:

```bash
docker compose stop db
```

**Starte igjen senere** - steg 1-3, 5 og 9 er engangsjobber, så du trenger bare:

```bash
docker compose up -d db
cd backend/SportForAlle.Api && dotnet run     # vindu 1
cd frontend && bun dev                        # vindu 2
```

**Slette alt og begynne på nytt**, inkludert databaseinnholdet:

```bash
docker compose down -v
```

Da må steg 5 kjøres om igjen, og ansattprofilen fra steg 9 må opprettes på nytt.

---

## Om de utleverte verdiene

`.env`-filen inneholder ekte nøkler til Auth0 og EmailJS. Tre ting er verdt å
vite:

1. **Dette er utviklingsverdier.** Auth0-oppsettet inneholder noen få manuelt
   opprettede testbrukere og ingen ekte personopplysninger. Registrering av nye
   brukere er slått av i Auth0, så antallet kontoer holder seg der.

2. **Nøklene skal byttes ut før systemet brukes på ekte.** Når dere tar over for
   alvor, sett opp deres egen Auth0-tenant og deres egen EmailJS-konto. Hva som
   må konfigureres i Auth0-dashbordet - applikasjon, API, callback-URL-er og
   hvorfor «Allow Offline Access» må være på - står i
   [`06-autentisering.md`](./06-autentisering.md). Alle variabelnavnene er
   dokumentert i `.env.example`.

3. **E-post går ut på ekte.** Oppfølgings-e-post til foresatte sendes gjennom
   EmailJS med de utleverte nøklene. Bruk aldri en ekte e-postadresse til en
   foresatt når dere tester - registrer en testadresse dere selv eier. Se
   [`09-lover-og-regler.md`](./09-lover-og-regler.md).

`.env` er utelatt fra Git og skal aldri sjekkes inn eller sendes i en kanal som
arkiverer innholdet.
