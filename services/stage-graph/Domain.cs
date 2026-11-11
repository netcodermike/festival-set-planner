using System.Text.Json;

internal static class Domain
{
    private record Set(string Id, string Stage, int Start, int End, int Weight);

    public static object Execute(JsonElement input)
    {
        var walk = input.GetProperty("walk").GetInt32();
        var sets = input.GetProperty("sets").EnumerateArray().Select(Parse)
            .OrderBy(set => set.End).ThenBy(set => set.Id, StringComparer.Ordinal).ToArray();
        if (walk is < 0 or > 1440 || sets.Length > 200 || sets.Select(set => set.Id).Distinct().Count() != sets.Length)
        {
            throw new ArgumentException("Invalid walk time, schedule size or duplicate set ID");
        }
        var scores = new long[sets.Length];
        var parents = new int[sets.Length];
        var best = -1;
        for (var index = 0; index < sets.Length; index++)
        {
            var current = sets[index];
            scores[index] = current.Weight;
            parents[index] = -1;
            for (var previous = 0; previous < index; previous++)
            {
                var earlier = sets[previous];
                var transfer = earlier.Stage == current.Stage ? 0 : walk;
                var score = scores[previous] + current.Weight;
                if (earlier.End + transfer <= current.Start && score > scores[index])
                {
                    scores[index] = score;
                    parents[index] = previous;
                }
            }
            if (best < 0 || scores[index] > scores[best])
            {
                best = index;
            }
        }
        var ids = new List<string>();
        for (var index = best; index >= 0; index = parents[index])
        {
            ids.Add(sets[index].Id);
        }
        ids.Reverse();
        return new { ids, totalWeight = best < 0 ? 0 : scores[best] };
    }

    private static Set Parse(JsonElement entry)
    {
        var id = entry.GetProperty("id").GetString();
        var stage = entry.GetProperty("stage").GetString();
        var start = entry.GetProperty("start").GetInt32();
        var end = entry.GetProperty("end").GetInt32();
        var weight = entry.GetProperty("weight").GetInt32();
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(stage)
            || start < 0 || end <= start || end > 1440 || weight < 0)
        {
            throw new ArgumentException("Each set needs an ID, stage, valid times and nonnegative weight");
        }
        return new Set(id, stage, start, end, weight);
    }
}
