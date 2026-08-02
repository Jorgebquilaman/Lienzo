using System.Text;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;

namespace Lienzo.Infrastructure.Services;

public class ReservationPdfGenerator : IReservationPdfGenerator
{
    private static readonly Encoding PdfEncoding = Encoding.Latin1;
    private const int MarginLeft = 40;
    private const int StartY = 790;
    private const int LineHeight = 20;

    public byte[] Generate(ReservationPdfModel model)
    {
        var contentStream = BuildContent(model);

        var objects = new List<(int Num, string Body)>
        {
            (1, "<< /Type /Catalog /Pages 2 0 R >>"),
            (2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>"),
            (3, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>"),
            (4, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
            (5, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"),
            (6, $"<< /Length {PdfEncoding.GetByteCount(contentStream)} >>\nstream\n{contentStream}\nendstream")
        };

        var pdf = new MemoryStream();
        pdf.Write(PdfEncoding.GetBytes("%PDF-1.4\n"));

        var offsets = new List<long>();
        foreach (var (num, body) in objects)
        {
            offsets.Add(pdf.Position);
            pdf.Write(PdfEncoding.GetBytes($"{num} 0 obj\n{body}\nendobj\n"));
        }

        var xrefStart = (int)pdf.Position;
        pdf.Write(PdfEncoding.GetBytes(BuildXref(objects.Count, offsets, xrefStart)));

        return pdf.ToArray();
    }

    private static string BuildXref(int count, List<long> offsets, int start)
    {
        var sb = new StringBuilder();
        sb.Append("xref\n");
        sb.Append($"0 {count + 1}\n");
        sb.Append("0000000000 65535 f \n");
        foreach (var offset in offsets)
            sb.Append($"{offset:0000000000} 00000 n \n");
        sb.Append("trailer\n");
        sb.Append($"<< /Size {count + 1} /Root 1 0 R >>\n");
        sb.Append("startxref\n");
        sb.Append($"{start}\n");
        sb.Append("%%EOF\n");
        return sb.ToString();
    }

    private static string BuildContent(ReservationPdfModel model)
    {
        var lines = new List<string>();
        int y = StartY;

        lines.Add("BT");
        lines.Add(Position(y));
        lines.Add("/F2 18 Tf");
        lines.Add("0 0 0 rg");
        lines.Add($"({Encode(model.Title)}) Tj");

        y -= 24;
        lines.Add(Position(y));
        lines.Add("/F1 10 Tf");
        lines.Add($"({Encode($"Lienzo - Reserva de Aula ({model.Status})")}) Tj");

        y -= 16;
        lines.Add(Position(y));
        lines.Add($"({Encode($"N° {model.ReservationId:N}")}) Tj");

        y -= 8;
        lines.Add("0.35 0.35 0.35 rg");
        lines.Add(Position(y));
        lines.Add($"{MarginLeft} {y} {495} 0.5 re f");
        lines.Add("0 0 0 rg");

        y -= 6;
        lines.Add(Position(y));
        lines.Add("(Datos de la reserva) Tj");

        y -= LineHeight;
        lines.Add(Position(y));
        lines.Add($"({Encode($"Solicitante: {model.UserName} ({model.UserEmail})")}) Tj");

        y -= LineHeight;
        lines.Add(Position(y));
        lines.Add($"({Encode($"Aula: {model.ClassroomName}")}) Tj");

        if (!string.IsNullOrEmpty(model.BuildingName))
        {
            y -= LineHeight;
            lines.Add(Position(y));
            lines.Add($"({Encode($"Edificio: {model.BuildingName} · Piso {model.Floor}")}) Tj");
        }

        y -= LineHeight;
        lines.Add(Position(y));
        lines.Add($"({Encode($"Fecha: {model.Date}")}) Tj");

        y -= LineHeight;
        lines.Add(Position(y));
        lines.Add($"({Encode($"Horario: {model.StartTime} - {model.EndTime}")}) Tj");

        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            y -= LineHeight;
            lines.Add(Position(y));
            lines.Add($"({Encode($"Descripción: {model.Description}")}) Tj");
        }

        if (model.Accessories is { Count: > 0 })
        {
            y -= 16;
            lines.Add(Position(y));
            lines.Add("/F2 12 Tf");
            lines.Add("(Accesorios solicitados) Tj");
            lines.Add("/F1 10 Tf");

            foreach (var acc in model.Accessories)
            {
                y -= LineHeight;
                lines.Add(Position(y));
                lines.Add($"({Encode($"- {acc}")}) Tj");
            }
        }

        lines.Add("ET");
        return string.Join("\n", lines);
    }

    private static string Position(int y) => $"1 0 0 1 {MarginLeft} {y} Tm";

    private static string Encode(string text)
    {
        text ??= "";
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (ch == '(') sb.Append("\\(");
            else if (ch == ')') sb.Append("\\)");
            else if (ch == '\\') sb.Append("\\\\");
            else if (ch == '\n' || ch == '\r') sb.Append(' ');
            else if (ch > 255) sb.Append('?');
            else sb.Append(ch);
        }
        return sb.ToString();
    }
}