using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyRazorApp.Pages.Rooms;

public class IndexModel : PageModel
{
    public IReadOnlyList<RoomCard> Rooms { get; private set; } = Array.Empty<RoomCard>();

    public void OnGet()
    {
        var statuses = new[]
        {
            "Đã đặt", "Đã đặt", "Đã đặt", "Đã đặt",
            "Đang ở", "Đang ở", "Đang ở",
            "Đang dọn", "Đang dọn",
            "Bảo trì", "Bảo trì"
        };
        var statusIndex = 0;
        var photos = new[]
        {
            "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32",
            "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0",
            "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32",
            "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0", "photo-1611892440504-42a792e24d32", "photo-1616594039964-ae9021a400a0"
        };

        Rooms = Enumerable.Range(0, 20).Select(i =>
        {
            var floor = i / 5 + 1;
            var roomNumber = floor * 100 + i % 5 + 1;
            var status = "Trống";
            if (roomNumber is 101 or 102 or 103 or 104) status = statuses[statusIndex++];
            else if (roomNumber is 105 or 201 or 202) status = statuses[statusIndex++];
            else if (roomNumber is 203 or 204) status = statuses[statusIndex++];
            else if (roomNumber is 205 or 301) status = statuses[statusIndex++];

            var type = floor == 1 ? "Superior" : floor == 2 ? "Deluxe" : floor == 3 ? "Premium Deluxe" : "Executive Suite";
            return new RoomCard(roomNumber, floor, type, status,
                850_000 + (floor - 1) * 350_000 + (i % 5) * 50_000,
                4.7 + (i % 4) * 0.1,
                $"https://images.unsplash.com/{photos[i]}?auto=format&fit=crop&w=900&q=80");
        }).ToList();
    }
}

public record RoomCard(int Number, int Floor, string Type, string Status, int Price, double Rating, string ImageUrl);
