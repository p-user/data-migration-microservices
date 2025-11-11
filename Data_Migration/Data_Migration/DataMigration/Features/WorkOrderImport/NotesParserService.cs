using Data_Migration.DataMigration.Dtos;
using Data_Migration.DataMigration.Exceptions;
using System.Text.RegularExpressions;

namespace Data_Migration.DataMigration.Features.WorkOrderImport
{
    public class NotesParserService : INotesParserService
    {
        private static readonly Regex DatePattern = new(@"\b(\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{2,4})\b", RegexOptions.Compiled
   );

        private static readonly Regex ClientPattern = new(
            @"(?:client|customer|for|visit to)[\s:]+([A-Z][a-z]+)\s+([A-Z][a-z]+)",   RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public WorkOrderProcessedDto ParseNotes(WorkOrderRawDto rawDto)
        {
            try
            {
                var (techFirst, techLast) = ParseTechnicianName(rawDto.TechnicianName);
                var (clientFirst, clientLast) = ExtractClientName(rawDto.Notes);
                var serviceDate = ExtractDate(rawDto.Notes);

                return new WorkOrderProcessedDto(
                    TechnicianFirstName: techFirst,
                    TechnicianLastName: techLast,
                    ClientFirstName: clientFirst,
                    ClientLastName: clientLast,
                    ServiceDate: serviceDate,
                    Notes: rawDto.Notes,
                    Total: rawDto.Total,
                    RowNumber: rawDto.RowNumber
                );
            }
            catch (Exception ex)
            {
                throw new ParsingException(
                    $"Failed to parse notes: {ex.Message}",
                    rawDto.RowNumber
                );
            }
        }

        private (string FirstName, string LastName) ParseTechnicianName(string fullName)
        {
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts.Length switch
            {
                >= 2 => (parts[0], string.Join(" ", parts.Skip(1))),
                1 => (parts[0], "Unknown"),
                _ => throw new ArgumentException("Invalid technician name")
            };
        }

        private (string FirstName, string LastName) ExtractClientName(string notes)
        {
            var match = ClientPattern.Match(notes);

            if (match.Success)
            {
                return (match.Groups[1].Value, match.Groups[2].Value);
            }

            // Fallback: try to find any capitalized name pattern
            var words = notes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var capitalizedWords = words.Where(w =>
                w.Length > 1 &&
                char.IsUpper(w[0]) &&
                w.Skip(1).All(c => char.IsLower(c) || !char.IsLetter(c))
            ).ToList();

            if (capitalizedWords.Count >= 2)
            {
                return (capitalizedWords[0], capitalizedWords[1]);
            }

            return ("Unknown", "Client");
        }

        private DateTime ExtractDate(string notes)
        {
            var match = DatePattern.Match(notes);

            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var date))
            {
                return date;
            }

            // some default date if no date found
            return DateTime.Today;
        }
    }
}
