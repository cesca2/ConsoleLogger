using Spectre.Console;
using static SessionLogger.Enums;
using SessionLogger.Models;
using SessionLogger.Controllers;

namespace SessionLogger;

internal class UserInterface
{
    private readonly DatabaseController _databaseController = new();

    void DisplayMessage(string message, string color = "yellow")
        {
            AnsiConsole.MarkupLine($"[{color}]{message}[/]");
        }

    bool ConfirmAction(string actionName, string color = "yellow")
        {
            var ConfirmAction = AnsiConsole.Confirm($"Are you sure you want to [{color}]{actionName}[/]?");

            return ConfirmAction;
        }
    internal void MainMenu()
    {

        DisplayMessage("Welcome to the Session Logger application!", "white");

        Console.ReadKey();

        _databaseController.SQLHandler("""
            CREATE TABLE IF NOT EXISTS sessions (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                type TEXT NOT NULL,
                date TEXT NOT NULL, 
                duration TEXT NOT NULL,
                start TEXT NOT NULL,
                end TEXT NOT NULL
            );

        """);

        _databaseController.SQLHandler("""
            INSERT INTO sessions (
                type,
                date, 
                duration,
                start,
                end
            )
            VALUES ('Python', '12/02/2025', '2h 30m', '12:30', '14:30');

        """);

        while (true)
        {
            Console.Clear();

            var actionChoice = AnsiConsole.Prompt(
                new SelectionPrompt<MenuAction>()
                .Title("Please select an action:")
                .UseConverter(e => System.Text.RegularExpressions.Regex.Replace(e.ToString(), "([a-z])([A-Z])", "$1 $2"))
                .AddChoices(Enum.GetValues<MenuAction>()));

            switch (actionChoice)
            {
                case MenuAction.ViewSessions:
                    ViewSessions();
                    break;
                case MenuAction.AddSession:
                    AddSession();
                    break;
                case MenuAction.DeleteSession:
                    DeleteSession();
                    break;
                case MenuAction.UpdateSession:
                    UpdateSession();
                    break;
            }


        }
    }

    void ViewSessions()
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);

        table.AddColumn("[yellow]Type[/]");
        table.AddColumn("[yellow]Date[/]");
        table.AddColumn("[yellow]Length[/]");

        var entries = _databaseController.GetAllRecords();

        foreach (var entry in entries)
        {
            table.AddRow(
                $"[cyan]{entry.SessionType}[/]",
                $"[blue]{entry.Date}[/]",
                $"[green]{entry.Duration}[/]"
                );
        }

        AnsiConsole.Write(table);
        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();
    }

    void AddSession()
    {
        var type= AnsiConsole.Ask<string>("Enter the [cyan]type[/] of session to add:");
        var date= AnsiConsole.Ask<DateTime>("Enter the [blue]date (XX/XX/XXXX)[/] of the session to add:");
        var startTime = AnsiConsole.Ask<DateTime>("Enter the [green]start time (XX:XX) [/] of the session to add:");
        var endTime = AnsiConsole.Ask<DateTime>("Enter the [green]end time (XX:XX)[/] of the session to add:");

        var newSession= new Session(type, date, startTime, endTime);
        newSession.DisplayDetails();

        if (ConfirmAction("add this session?"))
        {
            _databaseController.SQLHandler(
            $"""
                INSERT INTO sessions(type, date, duration, start, end) 
                VALUES 
                ('{newSession.SessionType}', 
                '{newSession.Date}', 
                '{newSession.Duration}' , 
                '{newSession.StartTime}', 
                '{newSession.EndTime}')
                ;
            """);
        
            DisplayMessage("Session added");

        }
        else
        {
            DisplayMessage("Addition of session cancelled", "red");
        }

        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();


    }

    void DeleteSession()
    {
        var deletionEntries = _databaseController.GetAllRecords();
        var sessionToDelete = AnsiConsole.Prompt(
                new SelectionPrompt<Session>()
                    .Title("Select a [red]session[/] to delete:")
                    .UseConverter(e => $"{e.SessionType}, {e.Date}, {e.Duration} ")
                    .AddChoices(deletionEntries));
        sessionToDelete.DisplayDetails();
        if (ConfirmAction("delete the above entry?", "red"))
        {
            _databaseController.SQLHandler(
            $"""
                DELETE FROM sessions
                WHERE id={sessionToDelete.Id};
            """
            );
        }
        else
        {
            DisplayMessage("Deletion of session cancelled", "red");
        }
        
        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();


    }

    void UpdateSession()
    {
        var updateEntries = _databaseController.GetAllRecords();
        var sessionToUpdate = AnsiConsole.Prompt(
                new SelectionPrompt<Session>()
                    .Title("Select a [yellow]session[/] to update:")
                    .UseConverter(e => $"{e.SessionType}, {e.Date}, {e.Duration} ")
                    .AddChoices(updateEntries));
        
        var type= AnsiConsole.Ask<string>("Enter the new [cyan]type[/] of session to add:");
        var date= AnsiConsole.Ask<DateTime>("Enter the new [blue]date (XX/XX/XXXX)[/] of the session to add:");
        var startTime = AnsiConsole.Ask<DateTime>("Enter the new [green]start time (XX:XX) [/] of the session to add:");
        var endTime = AnsiConsole.Ask<DateTime>("Enter the new [green]end time (XX:XX)[/] of the session to add:");

        var newSession= new Session(type, date, startTime, endTime);
        
        DisplayMessage("", "white");
        DisplayMessage("Replace");
        sessionToUpdate.DisplayDetails();
        DisplayMessage("with");
        newSession.DisplayDetails();
        
        if (ConfirmAction($"update this entry as detailed above?", "yellow"))
        {
            _databaseController.SQLHandler(
                $"""
                UPDATE sessions
                SET date='{newSession.Date}',
                type='{newSession.SessionType}',
                duration='{newSession.Duration}',
                start='{newSession.StartTime}',
                end='{newSession.EndTime}'
                WHERE id={sessionToUpdate.Id};
                """);
        }
        else
        {
            DisplayMessage("Update of session cancelled", "red");
        }

        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();

    }
}
