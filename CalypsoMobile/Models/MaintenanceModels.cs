using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CalypsoMobile.Models;

public partial class WorkOrder : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = "NORMAL"; // URGENT / NORMAL
    public string PriorityColor => Priority == "URGENT" ? "#EF4444" : "#F59E0B";

    [ObservableProperty]
    private bool _isClosed;

    public ObservableCollection<WorkOperation> Operations { get; set; } = new();

    public string ProgressText
    {
        get
        {
            int done = 0;
            foreach (var op in Operations)
                if (op.IsCompleted) done++;
            return $"{done}/{Operations.Count} Operații finalizate";
        }
    }
}

public partial class WorkOperation : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Title { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isCompleted;

    public string StatusLabel => IsCompleted ? "✅ FINALIZATĂ" : "⏳ ÎN LUCRU";
    public ObservableCollection<WorkActivity> Activities { get; set; } = new();
}

public partial class WorkActivity : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isClosed;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private TimeSpan _elapsedTime = TimeSpan.Zero;

    [ObservableProperty]
    private bool _isTimerRunning;

    public ObservableCollection<SparePart> SpareParts { get; set; } = new();
    public ObservableCollection<SupportDocument> Documents { get; set; } = new();
}

public partial class SparePart : ObservableObject
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "buc";
    public int StockAvailable { get; set; }

    [ObservableProperty]
    private int _usedQuantity;
}

public class SupportDocument
{
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string MockPdfContent { get; set; } = string.Empty;
}

public class ChatMessage
{
    public string Sender { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string RelatedOrderId { get; set; } = string.Empty;
    public bool IsMine { get; set; }
}
