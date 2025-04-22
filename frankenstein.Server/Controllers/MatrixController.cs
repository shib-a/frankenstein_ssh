using frankenstein.Server.DTOs.Matrix;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;

namespace frankenstein.Server.Controllers;

[ApiController]
[Route("matrix")]
public class MatrixController : ControllerBase
{
    private readonly MatrixService _matrixService;

    public MatrixController(MatrixService service)
    {
        _matrixService = service;
    }

    [HttpPost("table")]
    public MatrixReplyDTO SolveMatrixOne([FromBody] MatrixRequestDTO request)
    {
        var solution = _matrixService.SolveEquations(request);
        return solution;
    }

    [HttpPost("file")]
    public async Task<MatrixReplyDTO> HandleFileUpload(IFormFile file)
    {
        if (file == null || file.Length == 0) return new MatrixReplyDTO(ReplyEnum.FILE_NOT_FOUND, "No file uploaded.");

        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            if (!reader.EndOfStream)
            {
                try
                {
                    //if (file.FileName.IndexOf(".") <= 0 || file.FileName.Substring(file.FileName.IndexOf("."), file.FileName.Length)!=".txt"){ return new ReplyDTO(ReplyEnum.EMPTY_FILE, "The file has no data"); }
                    ;
                    var firstLine = await reader.ReadLineAsync();
                    var splitLine = firstLine.Split(' ');
                    var size = int.Parse(splitLine[0]);
                    var precision = double.Parse(string.Format(string.Concat("0", splitLine[1])),
                        System.Globalization.CultureInfo.InvariantCulture);
                    double[][] coefficients = new double[size][];
                    double[] values = new double[size];
                    for (int i = 0; i < size; i++)
                    {
                        coefficients[i] = new double[size];
                    }

                    int line_count = 0;
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var spl = line.Split(' ');
                        for (int i = 0; i < spl.Length - 1; i++)
                        {
                            coefficients[line_count][i] =
                                double.Parse(spl[i], System.Globalization.CultureInfo.InvariantCulture);
                        }

                        values[line_count] = double.Parse(spl[spl.Length - 1],
                            System.Globalization.CultureInfo.InvariantCulture);
                        line_count++;
                    }

                    var result = new MatrixRequestDTO();
                    result.Coefficients = coefficients;
                    result.Values = values;
                    result.Precision = precision;
                    var solution = _matrixService.SolveEquations(result);
                    return solution;
                }
                catch (Exception e)
                {
                    return new MatrixReplyDTO(ReplyEnum.ERROR, $"The file has invalid data");
                }

            }

            return new MatrixReplyDTO(ReplyEnum.EMPTY_FILE, "The file has no data");
        }
    }
}

