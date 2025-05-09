using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;


class HotelCapacity
{
    static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        var checks = GetCheks(guests)
            .OrderBy(x => x.Item1)
            .ThenBy(x => x.Item2)
            .ToList();

        var counter = 0;

        foreach (var check in checks)
        {
            counter += check.Item2;
            if (counter > maxCapacity) 
                return false;
        }

        return true;
    }

    static List<(DateTime, int)> GetCheks(List<Guest> guests)
    {
        var result = new List<(DateTime, int)>();

        foreach(var gue in guests)
        {
            var timeIn = DateTime.ParseExact(gue.CheckIn, "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture);
            var timeOut = DateTime.ParseExact(gue.CheckOut, "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture);

            result.Add((timeIn, 1));
            result.Add((timeOut, -1));
        }

        return result;
    }

    class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }


    static void Main()
    {
        int maxCapacity = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());


        List<Guest> guests = new List<Guest>();


        for (int i = 0; i < n; i++)
        {
            string line = Console.ReadLine();
            Guest guest = ParseGuest(line);
            guests.Add(guest);
        }


        bool result = CheckCapacity(maxCapacity, guests);


        Console.WriteLine(result ? "True" : "False");
    }


    //Простой парсер JSON-строки для объекта Guest
    static Guest ParseGuest(string json)
    {
        var guest = new Guest();


        // Извлекаем имя
        Match nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;


        // Извлекаем дату заезда
        Match checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;


        // Извлекаем дату выезда
        Match checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;


        return guest;
    }
}