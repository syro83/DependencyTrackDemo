# Waarom organisaties een open-source software-register nodig hebben

Moderne softwareontwikkeling is sterk afhankelijk van open-sourcecomponenten uit ecosystemen zoals NuGet en npm. Hoewel deze componenten de ontwikkelsnelheid verhogen, brengen zij ook risico’s met zich mee op het gebied van beveiliging en compliance.

Zonder een goede aanpak voor het beheer van de applicaties met externe componenten lopen organisaties verschillende risico’s:

- **De impact van kwetsbaarheden is moeilijk of niet te beoordelen**. Wanneer een nieuwe kwetsbaarheid wordt gepubliceerd, moeten organisaties snel kunnen vaststellen waar getroffen componenten worden gebruikt. Bekende incidenten zoals SolarWinds, Log4Shell en de Axios supply-chain attack laten de gevolgen zien van vertraagde zichtbaarheid en respons.
- **Licentierisico kan onopgemerkt blijven**. Ontwikkelaars kunnen pakketten introduceren met commercieel onwenselijke of verboden licenties. Daarnaast kunnen maintainers hun licentiemodel in de loop van de tijd wijzigen, waardoor routinematige upgrades direct juridische problemen veroorzaken. Recente voorbeelden zijn Fluent Assertions, AutoMapper en MediatR.
- **Vertragingen in softwareontwikkeling**. Bedrijfskritische applicaties kunnen afhankelijk zijn van verouderde, niet-ondersteunde of kwalitatief zwakke componenten, wat het operationele risico vergroot. Applicaties zijn dan moeilijker door te ontwikkelen, omdat het upgraden van cruciale oude componenten extra risico oplevert. Ook kan de technologiestack blijven steken op een end-of-life versie wanneer een component niet meer wordt onderhouden.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_3_c_9d7e69301f.png" alt="Software problems" width="450">
</p>
</br>
</br>

_Om deze redenen is het invoeren van een open-source software (OSS)-register een belangrijke aanpak om continu inzicht te krijgen in de componenten die binnen het softwarelandschap worden gebruikt._

De genoemde recente incidenten in de software supply chain laten zien dat afhankelijkheidsrisico’s niet theoretisch zijn, en de huidige mogelijkheden van AI bieden nieuwe manieren om deze risico’s verder te vergroten. Tegelijkertijd maakt het tempo van softwareontwikkeling het steeds lastiger om handmatig controle te houden. Veel grote organisaties hebben hiervoor al formele beheersmaatregelen ingericht, maar dezelfde noodzaak geldt ook voor middelgrote en kleinere organisaties. De drempel lijkt groot, maar met de juiste aanpak kan dit relatief eenvoudig worden ingericht en onderhouden.

## SBOM

Een fundamentele eerste stap is het opstellen van een **Software Bill of Materials (SBOM)** voor elke applicatie die wordt ontwikkeld. Een SBOM is een formele, machineleesbare inventaris van alle componenten, libraries, packages en modules waaruit een applicatie bestaat. Vergelijkbaar met een ingrediëntenlijst legt een SBOM vast wat er in een oplossing is opgenomen, inclusief relevante metadata zoals versies, leveranciers, onderlinge relaties en licentie-informatie. De SBOM vermeldt niet alleen de directe afhankelijkheden, maar ook alle transitieve afhankelijkheden waar die componenten zelf weer op steunen.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_1_b_3a69371133.png" alt="What is an SBOM" width="450">
</p>
</br>
</br>

SBOM's vormen inmiddels een essentieel bouwblok voor software supply-chainbeveiliging en compliance. Bij elke nieuwe softwareversie moet een actuele SBOM worden gegenereerd. Dit wordt gerealiseerd door het SBOM-generatieproces te integreren in bestaande CI/CD-pijplijnen, zodat dit automatisch en consistent plaatsvindt.

## Open-Source Software (OSS)-register

Een SBOM levert de noodzakelijke brondata, maar is op zichzelf niet voldoende. Om deze informatie effectief te gebruiken, is een centrale inventaris van alle open-sourcecomponenten nodig. Daarnaast is een proces nodig dat componentgegevens binnen het volledige applicatielandschap continu inleest en bijhoudt, analyseert en bewaakt. Typische eisen aan een dergelijk register zijn:

- Automatisch SBOM’s importeren vanuit CI/CD-pijplijnen voor alle relevante applicaties en diensten.
- Een centraal overzicht bieden van alle gebruikte softwarecomponenten, inclusief versie, licentietype en herkomst.
- Wijzigingen in OSS-licenties detecteren en waarschuwen wanneer een component een restrictiever licentiemodel krijgt.
- Kwetsbaarheden automatisch detecteren via gekoppelde bronnen (zoals NVD en OSS Index) en deze per project of applicatie inzichtelijk maken.
- Exact tonen waar kwetsbare componenten worden gebruikt, zodat impactanalyses snel kunnen worden uitgevoerd.
- Beleidsregels ondersteunen voor licentiecompliance, waaronder het markeren van verboden of onwenselijke licentietypen.
- Een notificatiemechanisme bieden (bijvoorbeeld e-mail, webhook of integratie met ticketsystemen) voor nieuwe kwetsbaarheden of licentierisico’s.
- Een dashboard bieden met realtime inzicht in risico’s, kwetsbaarheden, licenties en componenttrends.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_14_77f6a1a7da.png" alt="Key Takeaways" width="450">
</p>
</br>
</br>

### Dependency-Track

Een startpunt kan [Dependency-Track](https://dependencytrack.org/) van de OWASP Foundation zijn. Dit is een open-sourceplatform voor SBOM-analyse en risicobeheer van de software supply chain. Dependency-Track wordt veel gebruikt en is ontworpen om componenten te inventariseren, kwetsbaarheden te identificeren en beleid af te dwingen.

- **Open-sourceplatform**. Dependency-Track kan zonder softwarelicentiekosten worden ingevoerd, wat de instapdrempel verlaagt voor organisaties die hun supply-chaingovernance willen versterken.
- **Sterke governance- en analysemogelijkheden**. Het ondersteunt SBOM-inname, correlatie van kwetsbaarheidsinformatie, licentiemonitoring, beleidsevaluatie en impactanalyse over projecten en portfolio’s heen.
- **Integraties**. Het platform ondersteunt dashboards, notificaties, webhooks en integraties met externe systemen die door security-, engineering- en governanceteams worden gebruikt.
- **API-first architectuur**. Dankzij het API-gerichte ontwerp is het goed te integreren met CI/CD-pijplijnen, waaronder die van Azure DevOps en GitHub.
- **Eenvoudige implementatie**. Het platform is relatief lichtgewicht en kan worden uitgerold in gangbare cloud- of containeromgevingen.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_12_8253f9e207.png" alt="Dependency-Track" width="450">
</p>
</br>
</br>

Het is een goed, degelijk en toegankelijk startpunt voor het inrichten van OSS-governance. In de basis biedt het alles wat noodzakelijk is. Door het open API-first ontwerp is het mogelijk om het platform aan te vullen met aanvullende automatisering of interne services, bijvoorbeeld om project lifecycle management, SBOM-uploadprocessen of andere integraties beter te laten aansluiten op de gewenste behoeften. Een goed geimplementeerd CI/CD-proces met OSS-governance ziet er ongeveer als volgt uit.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_9_cd1e96f0e9.png" alt="CI/CD pipeline with DependencyTrack and Helper infographic" width="450">
</p>
</br>
</br>

Andere noemenswaardige SBOM-platformen en Software Composition Analysis (SCA)-tools zijn bijvoorbeeld Syft (OSS), FOSSA, Mend, Anchore Enterprise en Snyk.

## Governance

Technologie is slechts een onderdeel van een goed ingerichte softwareontwikkelstrategie. De werkelijke meerwaarde van Dependency-Track wordt pas bereikt wanneer de oplossing goed is ingebed binnen de organisatie en wordt ondersteund door een effectief _governanceproces_.

Goede governance omvat onder andere beleid voor het introduceren van nieuwe open-sourcecomponenten, het actueel houden van Software Bills of Materials (SBOM's), procedures voor risicoacceptatie en de integratie van beveiligingsmonitoring in de volledige softwareontwikkelcyclus. Opvolging en eigenaarschap hierin zijn cruciaal. Daarnaast zorgen periodieke rapportages, dashboards en managementreviews ervoor dat risico's binnen de software supply chain zichtbaar blijven voor zowel technische teams als besluitvormers.

Dit is een continu proces dat regelmatig moet worden geëvalueerd en aangepast aan veranderende technologieën, bedreigingen en bedrijfsbehoeften. Alleen door een benadering van zowel technologie als governance kunnen organisaties effectief de risico's van open-sourcecomponenten beheren en de voordelen van moderne softwareontwikkeling benutten.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_16b_2375354105.png" alt="Governance" width="450">
</p>
</br>
</br>

## Key Takeaways

Het is belangrijk dat organisaties een open-source software-register implementeren en onderhouden. Het register moet SBOM’s automatisch kunnen importeren vanuit CI/CD-pijplijnen, een centraal overzicht bieden van alle gebruikte componenten, wijzigingen in licenties detecteren, kwetsbaarheden automatisch identificeren en beleidsregels ondersteunen voor licentiecompliance.

Dependency-Track is een open-sourceplatform dat deze functionaliteiten biedt en kan worden geïntegreerd met CI/CD-pijplijnen. Het is echter belangrijk om te realiseren dat technologie slechts één onderdeel is van een succesvolle softwareontwikkelstrategie en dat een effectief governanceproces essentieel is voor het behalen van de werkelijke meerwaarde van Dependency-Track.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_b_68b561eb05.png" alt="DependencyTrack Example" width="450">
</p>
</br>
</br>

## Tutorial

Voor een referentie-implementatie van de uitrol, configuratie en integratie van Dependency-Track met SBOM-monitoring in Azure DevOps wordt verwezen naar mijn implementatiehandleiding op GitHub. Zie [https://github.com/syro83/DependencyTrackDemo](https://github.com/syro83/DependencyTrackDemo).

De handleiding beschrijft hoe SBOM-generatie, publicatie en monitoring structureel kunnen worden opgenomen in een Azure DevOps-omgeving. Daarnaast behandelt de handleiding de uitrol en basisconfiguratie van Dependency-Track op Azure. Aan de hand van een demoapplicatie wordt toegelicht hoe inzicht ontstaat in gebruikte componenten, licentierisico’s en bekende kwetsbaarheden.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_8_211db28229.png" alt="DependencyTrack Example" width="450">
</p>
</br>
</br>

## Implementatie en advies

Wij ondersteunen organisaties bij het implementeren van een OSS-register en het verankeren van een governanceproces. Daarbij combineren wij onze technische implementatiekennis met ervaring in organisatorische inrichtingen. Het doel is om Dependency-Track, of een ander OSS-register, en de bijbehorende processen een duurzaam onderdeel te maken van het softwareontwikkelproces. Wij zijn experts in het softwareontwikkelproces en kunnen ondersteunen bij bredere verbeteringen binnen een organisatie.

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_10_01dcc69c4e.png" alt="OSS/DependencyTrack implementatie en Advies" width="450">
</p>
</br>
</br>

---

</br>
<p align="center">
    <img src="https://cms.xprtz.dev/uploads/image_17_1235593b99.png" alt="DependencyTrack Example" width="450">
</p>
</br>
</br>
