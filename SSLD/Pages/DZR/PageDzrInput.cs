using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SSLD.Data;
using SSLD.Data.DZZR;
using SSLD.Parsers.DZZR;
using SSLD.Tools;

namespace SSLD.Pages.DZR;

[Authorize(Roles = SD.Role_User)]
public partial class PageDzrInput
{
    [Inject] public ApplicationDbContext Db { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }
    private IEnumerable<OperatorGis> _gisList;
    private string _currentFile;
    private bool _isReady;
    private List<FileMessage> _messages = new();

    protected override async Task OnInitializedAsync()
    {
        _gisList = await Db.OperatorGises.ToListAsync();
    }

    public class FileMessage
    {
        public string Filename { get; set; }
        public string Message { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int SendedCount { get; set; }
    }

    private async Task LoadExcelFiles(InputFileChangeEventArgs e)
    {
        _isReady = false;
        _messages = new List<FileMessage>();
        if (e.FileCount > 150)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Слишком много файлов",
                Detail = "Максимум возможна обработка до 100 файлов за раз",
                Duration = 5000
            });
            _isReady = true;
            return;
        }
        foreach (IBrowserFile file in e.GetMultipleFiles(150))
        {
            _messages.Add(new FileMessage()
            {
                Filename = file.Name
            });
        }
        foreach (IBrowserFile file in e.GetMultipleFiles(150))
        {
            _currentFile = file.Name;
            ExcelDzPreParser parser = new ExcelDzPreParser(file, NotificationService, _gisList);
            await parser.ParseFile();
            DZRValueList reviewList = parser.GetResult();
            await SaveResult(reviewList);
            StateHasChanged();
        }
        _currentFile = null;
        _isReady = true;
    }

    private async Task SaveResult(DZRValueList valueList)
    {
        FileMessage message = _messages.FirstOrDefault(x => x.Filename == valueList.Filename);
        if (message != null)
        {
            message.SendedCount = valueList.Values.Count;
            await CreateOrUpdate(valueList, message);
        }
    }

    private async Task CreateOrUpdate(DZRValueList list, FileMessage message)
    {
        foreach (OperatorResource value in list.Values)
        {
            OperatorResource dbVal = await Db.OperatorResources
                .Where(x =>
                    x.OperatorGisId == value.OperatorGisId &&
                    x.ReportDate == value.ReportDate &&
                    x.SupplyDate == value.SupplyDate &&
                    x.Type == value.Type)
                .Include(x => x.Hours)
                .FirstOrDefaultAsync();
            if (dbVal != null)
            {
                dbVal.Hours = value.Hours;
                _ = Db.Update(dbVal);
                message.UpdatedCount++;
            }
            else
            {
                _ = await Db.AddAsync(value);
                message.CreatedCount++;
            }
            _ = await Db.SaveChangesAsync();
        }
    }
}