Back-end development blok 6


├── /                           ← Presentatie laag
│   ├── Program.cs              ← Startpunt
│   └── Menus.cs                ← Alle menu’s (hoofdmenu, reserveringen, login)
│
├── BusinessLogic/              ← Regels & beslissingen
│   ├── Models.cs               ← Data-objecten (Kamer, Reservering, Gebruiker)
│   └── Services.cs             ← Logica (beschikbaarheid checken, reserveren, login)
│
├── DataAccess/                 ← Alles wat met database te maken heeft
│   └── Repository.cs           ← SQL opdrachten (kamer ophalen, reservering opslaan)