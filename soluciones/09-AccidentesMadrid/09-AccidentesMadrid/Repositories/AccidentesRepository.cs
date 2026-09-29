using System.Globalization;
using System.Text;
using AccidentesMadrid.Mappers;
using AccidentesMadrid.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace AccidentesMadrid.Repositories;

public sealed class AccidentesRepository(string rutaCsv) : IAccidentesRepository
{
    public async Task<IReadOnlyList<Accidente>> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            using var reader = new StreamReader(rutaCsv, Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ";"              // El CSV real usa punto y coma
            });

            csv.Context.RegisterClassMap<AccidenteMapper>();

            return csv.GetRecords<Accidente>().ToList();
        });
    }
}
