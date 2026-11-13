# Calypso Mobile - Aplicație Mentenanță (Avalonia UI + .NET 8 / MAUI Essentials)

Soluție completă pentru **Visual Studio 2022** care implementează interfața mobilă industrială **Calypso Mobile** (temă Dark Mode `#121417` + accent portocaliu `#FF6F00`).

## Structura Soluției
- **CalypsoMobile**: Proiectul principal (.NET 8) cu modelele, ViewModel-ul (`CommunityToolkit.Mvvm`) și toate ecranele XAML (`MainView.axaml`).
- **CalypsoMobile.Android**: Proiect Android (.NET 8 Android + `Microsoft.Maui.Essentials` pentru acces la cameră/senzori).
- **CalypsoMobile.iOS**: Proiect iOS (.NET 8 iOS + `Microsoft.Maui.Essentials`).
- **CalypsoMobile.Desktop**: Proiect Windows/Desktop pentru testare rapidă direct în Visual Studio 2022 fără emulator.

## Funcționalități Incluse
1. **Comenzi**: Filtrare (`Toate`, `Urgente`, `În lucru`), căutare rapidă și deschidere operații.
2. **Operații**: Vizualizare status operații, deschidere activitate și întoarcere la comenzi.
3. **Activități**: Pontaj timp (`Start` / `Pauză` / `Stop`), gestionare note operator, acces la documente și piese, închidere activitate cu întrebare modală pentru închiderea comenzii.
4. **Piese de schimb**: Incrementare/decrementare cantitate consumată cu verificare stoc și salvare.
5. **Documente de suport**: Listă documente și vizualizator PDF integrat cu întoarcere la activitate.
6. **Mesaje**: Primire și trimitere mesaje către dispecerat.
