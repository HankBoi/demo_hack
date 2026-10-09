using System.Text;
using TenderEvidenceChecker.Api.Data;

namespace TenderEvidenceChecker.Api.Services;

public static class CsvExporter
{
    private static readonly string[] Columns =
    [
        "draft_label", "analysis_label", "finding_id", "kind", "category", "severity", "statement",
        "explanation", "possible_impact", "next_step", "requirement_class", "mandatory_label",
        "source_file", "source_page", "source_quote", "source_status", "evidence_file", "evidence_page",
        "evidence_quote", "evidence_status", "human_decision", "human_note", "review_status", "limitations"
    ];

    private static readonly Dictionary<string, Dictionary<string, string>> Text = new()
    {
        ["en"] = new()
        {
            ["draft_label"] = "Draft label", ["analysis_label"] = "Analysis", ["finding_id"] = "Finding ID",
            ["kind"] = "Type", ["category"] = "Category", ["severity"] = "Severity", ["statement"] = "Statement",
            ["explanation"] = "Explanation", ["possible_impact"] = "Possible impact", ["next_step"] = "Suggested next step",
            ["requirement_class"] = "Requirement class", ["mandatory_label"] = "Mandatory label",
            ["source_file"] = "Source file", ["source_page"] = "Source page", ["source_quote"] = "Exact quote",
            ["source_status"] = "Source check", ["evidence_file"] = "Possible supporting file",
            ["evidence_page"] = "Supporting page", ["evidence_quote"] = "Supporting quote",
            ["evidence_status"] = "Evidence suggestion", ["human_decision"] = "Human decision",
            ["human_note"] = "Human note", ["review_status"] = "Review status", ["limitations"] = "Limitations",
            ["draft"] = "AI-assisted draft / human-reviewed", ["sample"] = " (sample mode)",
            ["verified"] = "quote verified on source page", ["unverified"] = "source not verified",
            ["unreadable"] = "unreadable page"
        },
        ["az"] = new()
        {
            ["draft_label"] = "Layihə qeydi", ["analysis_label"] = "Analiz", ["finding_id"] = "Tapıntı ID",
            ["kind"] = "Növ", ["category"] = "Kateqoriya", ["severity"] = "Əhəmiyyət", ["statement"] = "Ifadə",
            ["explanation"] = "İzah", ["possible_impact"] = "Mümkün təsir", ["next_step"] = "Təklif olunan növbəti addım",
            ["requirement_class"] = "Tələb sinfi", ["mandatory_label"] = "Məcburilik",
            ["source_file"] = "Mənbə faylı", ["source_page"] = "Mənbə səhifəsi", ["source_quote"] = "Dəqiq sitat",
            ["source_status"] = "Mənbə yoxlaması", ["evidence_file"] = "Mümkün dəstəkləyici fayl",
            ["evidence_page"] = "Dəstəkləyici səhifə", ["evidence_quote"] = "Dəstəkləyici sitat",
            ["evidence_status"] = "Sübut təklifi", ["human_decision"] = "İnsan qərarı",
            ["human_note"] = "İnsan qeydi", ["review_status"] = "Baxış statusu", ["limitations"] = "Məhdudiyyətlər",
            ["draft"] = "Süni intellekt köməkli layihə / insan baxışı", ["sample"] = " (nümunə rejimi)",
            ["verified"] = "sitat mənbə səhifəsində yoxlanıldı", ["unverified"] = "mənbə təsdiqlənmədi",
            ["unreadable"] = "oxunmayan səhifə"
        },
        ["ru"] = new()
        {
            ["draft_label"] = "Пометка черновика", ["analysis_label"] = "Анализ", ["finding_id"] = "ID находки",
            ["kind"] = "Тип", ["category"] = "Категория", ["severity"] = "Важность", ["statement"] = "Формулировка",
            ["explanation"] = "Пояснение", ["possible_impact"] = "Возможное влияние", ["next_step"] = "Предлагаемый следующий шаг",
            ["requirement_class"] = "Класс требования", ["mandatory_label"] = "Обязательность",
            ["source_file"] = "Файл-источник", ["source_page"] = "Страница источника", ["source_quote"] = "Точная цитата",
            ["source_status"] = "Проверка источника", ["evidence_file"] = "Возможный подтверждающий файл",
            ["evidence_page"] = "Страница подтверждения", ["evidence_quote"] = "Цитата подтверждения",
            ["evidence_status"] = "Предложение по подтверждению", ["human_decision"] = "Решение человека",
            ["human_note"] = "Комментарий человека", ["review_status"] = "Статус проверки", ["limitations"] = "Ограничения",
            ["draft"] = "Черновик с помощью ИИ / проверено человеком", ["sample"] = " (демонстрационный режим)",
            ["verified"] = "цитата найдена на странице источника", ["unverified"] = "источник не подтверждён",
            ["unreadable"] = "нечитаемая страница"
        }
    };

    private static readonly Dictionary<string, Dictionary<string, string>> Values = new()
    {
        ["en"] = new()
        {
            ["requirement"] = "requirement", ["risk"] = "risk finding",
            ["required_document"] = "required document", ["deadline"] = "deadline or timeline", ["technical_mismatch"] = "technical match to check",
            ["contract_terms"] = "contract terms", ["conflicting_unclear"] = "conflicting or unclear", ["other"] = "other",
            ["high"] = "high", ["medium"] = "medium", ["low"] = "low", ["uncertain"] = "uncertain",
            ["possible_match"] = "possible supporting document", ["possible_mismatch"] = "possible mismatch",
            ["not_found"] = "not found in uploaded files", ["unclear"] = "unclear", ["expired_date_detected"] = "expiry-looking date seen",
            ["confirm"] = "confirmed", ["edit"] = "edited", ["reject"] = "rejected", ["not_applicable"] = "not applicable", ["missing"] = "marked as not in uploaded files",
            ["not_reviewed"] = "not reviewed", ["evidence_suggested"] = "evidence suggested", ["confirmed"] = "confirmed", ["changed"] = "changed",
            ["rejected"] = "rejected", ["expired"] = "expiry-looking date seen"
        },
        ["az"] = new()
        {
            ["requirement"] = "tələb", ["risk"] = "risk tapıntısı",
            ["required_document"] = "tələb olunan sənəd", ["deadline"] = "son tarix və ya müddət", ["technical_mismatch"] = "yoxlanmalı texniki uyğunluq",
            ["contract_terms"] = "müqavilə şərtləri", ["conflicting_unclear"] = "ziddiyyətli və ya qeyri-müəyyən", ["other"] = "digər",
            ["high"] = "yüksək", ["medium"] = "orta", ["low"] = "aşağı", ["uncertain"] = "qeyri-müəyyən",
            ["possible_match"] = "mümkün dəstəkləyici sənəd", ["possible_mismatch"] = "mümkün uyğunsuzluq",
            ["not_found"] = "yüklənmiş fayllarda tapılmadı", ["unclear"] = "qeyri-müəyyən", ["expired_date_detected"] = "bitmə kimi görünən tarix müşahidə olundu",
            ["confirm"] = "təsdiqlənib", ["edit"] = "redaktə edilib", ["reject"] = "rədd edilib", ["not_applicable"] = "tətbiq olunmur", ["missing"] = "yüklənmişlərdə yoxdur kimi qeyd edilib",
            ["not_reviewed"] = "baxılmayıb", ["evidence_suggested"] = "sübut təklif olunub", ["confirmed"] = "təsdiqlənib", ["changed"] = "dəyişdirilib",
            ["rejected"] = "rədd edilib", ["expired"] = "bitmə kimi görünən tarix müşahidə olundu"
        },
        ["ru"] = new()
        {
            ["requirement"] = "требование", ["risk"] = "риск",
            ["required_document"] = "необходимый документ", ["deadline"] = "срок или график", ["technical_mismatch"] = "техническое соответствие для проверки",
            ["contract_terms"] = "условия договора", ["conflicting_unclear"] = "противоречиво или неясно", ["other"] = "другое",
            ["high"] = "высокая", ["medium"] = "средняя", ["low"] = "низкая", ["uncertain"] = "неопределённая",
            ["possible_match"] = "возможный подтверждающий документ", ["possible_mismatch"] = "возможное несоответствие",
            ["not_found"] = "не найдено в загруженных файлах", ["unclear"] = "неясно", ["expired_date_detected"] = "замечена дата, похожая на окончание срока",
            ["confirm"] = "подтверждено", ["edit"] = "изменено", ["reject"] = "отклонено", ["not_applicable"] = "не применимо", ["missing"] = "отмечено как отсутствующее в загруженных файлах",
            ["not_reviewed"] = "не проверено", ["evidence_suggested"] = "предложено подтверждение", ["confirmed"] = "подтверждено", ["changed"] = "изменено",
            ["rejected"] = "отклонено", ["expired"] = "замечена дата, похожая на окончание срока"
        }
    };

    public static byte[] Build(AnalysisEntity analysis, string? language = null)
    {
        var lang = language is not null && Text.ContainsKey(language.ToLowerInvariant()) ? language.ToLowerInvariant() : "en";
        var text = Text[lang];
        string Word(string? key) => key is null ? "" : Values[lang].TryGetValue(key, out var word) ? word : key;

        var files = analysis.Files.ToDictionary(file => file.Id);
        var limitations = string.Join("; ", analysis.Files
            .SelectMany(file => file.Pages.Where(page => !page.Usable).Select(page => $"{file.OriginalName} p.{page.PageNumber}: {text["unreadable"]}")));
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', Columns.Select(column => Escape(text[column]))));
        var draft = text["draft"] + (analysis.IsSample ? text["sample"] : "");

        foreach (var requirement in analysis.Requirements.OrderBy(item => item.PageNumber).ThenBy(item => item.Statement))
        {
            var matches = requirement.Matches.Count == 0 ? [null] : requirement.Matches.Cast<EvidenceMatchEntity?>().ToList();
            foreach (var match in matches)
            {
                StoredFileEntity? evidenceFile = null;
                if (match?.EvidenceFileId is Guid evidenceId)
                {
                    files.TryGetValue(evidenceId, out evidenceFile);
                }

                StoredFileEntity? sourceFile = null;
                if (requirement.SourceFileId is Guid sourceId)
                {
                    files.TryGetValue(sourceId, out sourceFile);
                }

                string?[] cells =
                [
                    draft,
                    analysis.Label,
                    requirement.Id.ToString(),
                    Word(requirement.Kind),
                    Word(requirement.Category),
                    Word(requirement.QuoteVerified ? requirement.Severity : "uncertain"),
                    requirement.EditedStatement ?? requirement.Statement,
                    requirement.Explanation,
                    requirement.PossibleImpact,
                    requirement.NextStep,
                    requirement.RequirementClass,
                    requirement.MandatoryLabel,
                    sourceFile?.OriginalName,
                    requirement.PageNumber?.ToString(),
                    requirement.QuoteVerified ? requirement.Quote : "",
                    requirement.QuoteVerified ? text["verified"] : text["unverified"],
                    evidenceFile?.OriginalName,
                    match?.PageNumber?.ToString(),
                    match is { QuoteVerified: true } ? match.Quote : "",
                    match is null ? "" : Word(match.Status),
                    Word(requirement.ReviewerDecision),
                    requirement.HumanNote,
                    Word(requirement.ReviewStatus),
                    limitations
                ];
                builder.AppendLine(string.Join(',', cells.Select(Escape)));
            }
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(builder.ToString());
        var bytes = new byte[preamble.Length + body.Length];
        preamble.CopyTo(bytes, 0);
        body.CopyTo(bytes, preamble.Length);
        return bytes;
    }

    public static string Escape(string? value)
    {
        var text = value ?? "";
        if (text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            text = "'" + text;
        }

        return "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
