using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CalypsoMobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalypsoMobile.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;

    // Navigation state: "Orders", "Operations", "Activity", "SpareParts", "Documents", "PdfViewer", "Messages"
    [ObservableProperty]
    private string _currentScreen = "Orders";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = "Toate";

    [ObservableProperty]
    private WorkOrder? _selectedOrder;

    [ObservableProperty]
    private WorkOperation? _selectedOperation;

    [ObservableProperty]
    private WorkActivity? _selectedActivity;

    [ObservableProperty]
    private SupportDocument? _selectedDocument;

    [ObservableProperty]
    private bool _showCloseOrderModal;

    [ObservableProperty]
    private string _newNoteText = string.Empty;

    [ObservableProperty]
    private string _newMessageText = string.Empty;

    [ObservableProperty]
    private string _statusBanner = string.Empty;

    public ObservableCollection<WorkOrder> AllOrders { get; } = new();
    public ObservableCollection<WorkOrder> FilteredOrders { get; } = new();
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public MainViewModel()
    {
        SeedMockData();
        ApplyFilter();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) =>
        {
            if (SelectedActivity is { IsTimerRunning: true })
            {
                SelectedActivity.ElapsedTime = SelectedActivity.ElapsedTime.Add(TimeSpan.FromSeconds(1));
            }
        };
        _timer.Start();
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();
    partial void OnSelectedFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredOrders.Clear();
        var items = AllOrders.Where(o =>
            (SelectedFilter == "Toate" ||
             (SelectedFilter == "Urgente" && o.Priority == "URGENT") ||
             (SelectedFilter == "InLucru" && !o.IsClosed)) &&
            (string.IsNullOrWhiteSpace(SearchQuery) ||
             o.Id.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
             o.MachineName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)));

        foreach (var item in items)
            FilteredOrders.Add(item);
    }

    [RelayCommand]
    private void SetFilter(string filter) => SelectedFilter = filter;

    [RelayCommand]
    private void NavigateTab(string tab)
    {
        ShowCloseOrderModal = false;
        CurrentScreen = tab;
    }

    [RelayCommand]
    private void OpenOrder(WorkOrder order)
    {
        SelectedOrder = order;
        CurrentScreen = "Operations";
    }

    [RelayCommand]
    private void BackToOrders()
    {
        ApplyFilter();
        CurrentScreen = "Orders";
    }

    [RelayCommand]
    private void OpenOperation(WorkOperation operation)
    {
        SelectedOperation = operation;
        SelectedActivity = operation.Activities.FirstOrDefault();
        CurrentScreen = "Activity";
    }

    [RelayCommand]
    private void BackToOperations()
    {
        if (SelectedActivity != null)
            SelectedActivity.IsTimerRunning = false;
        CurrentScreen = "Operations";
    }

    [RelayCommand]
    private void ToggleTimer(string action)
    {
        if (SelectedActivity == null) return;
        if (action == "START") SelectedActivity.IsTimerRunning = true;
        else if (action == "PAUSE") SelectedActivity.IsTimerRunning = false;
        else if (action == "STOP")
        {
            SelectedActivity.IsTimerRunning = false;
            ShowNotification("Pontaj oprit.");
        }
    }

    [RelayCommand]
    private void AddNote()
    {
        if (SelectedActivity == null || string.IsNullOrWhiteSpace(NewNoteText)) return;
        string timestamp = DateTime.Now.ToString("HH:mm");
        SelectedActivity.Notes += $"\n• [{timestamp}] {NewNoteText.Trim()}";
        NewNoteText = string.Empty;
        ShowNotification("Notă salvată.");
    }

    [RelayCommand]
    private void OpenSpareParts() => CurrentScreen = "SpareParts";

    [RelayCommand]
    private void IncrementPart(SparePart part)
    {
        if (part.UsedQuantity < part.StockAvailable)
            part.UsedQuantity++;
    }

    [RelayCommand]
    private void DecrementPart(SparePart part)
    {
        if (part.UsedQuantity > 0)
            part.UsedQuantity--;
    }

    [RelayCommand]
    private void SaveSpareParts()
    {
        ShowNotification("Consum piese de schimb salvat!");
        CurrentScreen = "Activity";
    }

    [RelayCommand]
    private void OpenDocuments() => CurrentScreen = "Documents";

    [RelayCommand]
    private void OpenPdfDocument(SupportDocument doc)
    {
        SelectedDocument = doc;
        CurrentScreen = "PdfViewer";
    }

    [RelayCommand]
    private void BackToDocuments() => CurrentScreen = "Documents";

    [RelayCommand]
    private void BackToActivity() => CurrentScreen = "Activity";

    [RelayCommand]
    private void RequestCloseActivity()
    {
        if (SelectedActivity == null) return;
        SelectedActivity.IsTimerRunning = false;
        SelectedActivity.IsClosed = true;

        if (SelectedOperation != null && SelectedOperation.Activities.All(a => a.IsClosed))
        {
            SelectedOperation.IsCompleted = true;
        }

        // Întrebare: închidere comandă?
        ShowCloseOrderModal = true;
    }

    [RelayCommand]
    private void ConfirmCloseActivityOnly()
    {
        ShowCloseOrderModal = false;
        ShowNotification("Activitate închisă. Comanda rămâne deschisă.");
        CurrentScreen = "Operations";
    }

    [RelayCommand]
    private void ConfirmCloseOrderToo()
    {
        if (SelectedOrder != null)
        {
            SelectedOrder.IsClosed = true;
            foreach (var op in SelectedOrder.Operations)
                op.IsCompleted = true;
        }
        ShowCloseOrderModal = false;
        ShowNotification($"Comanda {SelectedOrder?.Id} a fost închisă complet!");
        ApplyFilter();
        CurrentScreen = "Orders";
    }

    [RelayCommand]
    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(NewMessageText)) return;
        Messages.Add(new ChatMessage
        {
            Sender = "Eu (Operator)",
            Timestamp = DateTime.Now.ToString("HH:mm"),
            Text = NewMessageText.Trim(),
            RelatedOrderId = SelectedOrder?.Id ?? "#1042",
            IsMine = true
        });
        NewMessageText = string.Empty;
    }

    private void ShowNotification(string msg)
    {
        StatusBanner = msg;
    }

    private void SeedMockData()
    {
        var doc1 = new SupportDocument
        {
            Title = "Manual Presă Hidraulică P-04 (Cap. 3)",
            FileName = "Manual_Presa_P04_Cap3.pdf",
            FileSize = "2.4 MB",
            MockPdfContent = "PROCEDURĂ STANDARD MENTENANȚĂ - PRESĂ P-04\n\n1. Opriți alimentarea electrică și blocați circuitul hidraulic (LOTO).\n2. Verificați manometrul principal: presiunea reziduală trebuie să fie 0 bar.\n3. Demontați flanșa cilindrului folosind cheie dinamometrică (cuplu 120 Nm).\n4. Înlocuiți garnitura GRN-502 și lubrifiați cu ulei H46 înainte de montaj."
        };

        var doc2 = new SupportDocument
        {
            Title = "Schemă Hidraulică Circuit Principal",
            FileName = "Schema_Hidraulica_2025.pdf",
            FileSize = "5.1 MB",
            MockPdfContent = "SCHEMĂ CIRCUIT HIDRAULIC - REVIZIA 2025\n\n• Pompă principală: P1 - 250 bar max\n• Supapă de siguranță: V-104 (reglată la 210 bar)\n• Cilindru lucru: C-01 (cursă 400 mm, etanșare dublă GRN-502)"
        };

        var act1 = new WorkActivity
        {
            Id = "ACT-201",
            Title = "Demontare cilindru principal și înlocuire garnitură",
            Notes = "• [09:45] Verificat presiune reziduală = 0 bar.",
            ElapsedTime = TimeSpan.FromMinutes(18)
        };
        act1.SpareParts.Add(new SparePart { Code = "GRN-502", Name = "Garnitură etanșare 50mm", Unit = "buc", StockAvailable = 12, UsedQuantity = 2 });
        act1.SpareParts.Add(new SparePart { Code = "UL-H46", Name = "Ulei hidraulic H46", Unit = "L", StockAvailable = 45, UsedQuantity = 5 });
        act1.Documents.Add(doc1);
        act1.Documents.Add(doc2);

        var act2 = new WorkActivity
        {
            Id = "ACT-202",
            Title = "Aerisire instalație și test presiune 180 bar",
            Notes = "• Pregătit manometru digital etalonat."
        };
        act2.Documents.Add(doc2);

        var order1 = new WorkOrder
        {
            Id = "#1042",
            MachineName = "Presă Hidraulică P-04",
            Location = "Hala B, Linia 2",
            Priority = "URGENT"
        };
        order1.Operations.Add(new WorkOperation { Id = "OP-1", Sequence = 1, Title = "Izolare și verificare presiune", IsCompleted = true });
        var op2 = new WorkOperation { Id = "OP-2", Sequence = 2, Title = "Înlocuire garnitură cilindru", IsCompleted = false };
        op2.Activities.Add(act1);
        order1.Operations.Add(op2);

        var op3 = new WorkOperation { Id = "OP-3", Sequence = 3, Title = "Testare funcționare în sarcină", IsCompleted = false };
        op3.Activities.Add(act2);
        order1.Operations.Add(op3);

        var order2 = new WorkOrder
        {
            Id = "#1045",
            MachineName = "Bandă Transportoare T-11",
            Location = "Depozit Central",
            Priority = "NORMAL"
        };
        var op21 = new WorkOperation { Id = "OP-10", Sequence = 1, Title = "Tensionare covor cauciuc și verificare role" };
        var act21 = new WorkActivity { Id = "ACT-301", Title = "Aliniere tambur motor și ungere rulmenți" };
        act21.SpareParts.Add(new SparePart { Code = "RUL-6205", Name = "Rulment radial SKF 6205", Unit = "buc", StockAvailable = 8, UsedQuantity = 0 });
        act21.Documents.Add(doc1);
        op21.Activities.Add(act21);
        order2.Operations.Add(op21);

        AllOrders.Add(order1);
        AllOrders.Add(order2);

        Messages.Add(new ChatMessage
        {
            Sender = "Dispecer Central",
            Timestamp = "10:15",
            Text = "Atenție la comanda #1042, piesa GRN-502 este pregătită pe raftul C4 în magazie.",
            RelatedOrderId = "#1042",
            IsMine = false
        });
        Messages.Add(new ChatMessage
        {
            Sender = "Eu (Operator)",
            Timestamp = "10:18",
            Text = "Am preluat garniturile și uleiul H46, am început demontarea.",
            RelatedOrderId = "#1042",
            IsMine = true
        });
    }
}
