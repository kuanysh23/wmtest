using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficLight.Models
{
    public class MyDb
    {
        private ConcurrentDictionary<Guid, object> Data { get; set; }

        public static readonly ReadOnlyDictionary<string, int> STRING_DIGITS_MAPPING = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>()
            {
                { "1110111", 0 },
                { "0010010", 1 },
                { "1011101", 2 },
                { "1011011", 3 },
                { "0111010", 4 },
                { "1101011", 5 },
                { "1101111", 6 },
                { "1010010", 7 },
                { "1111111", 8 },
                { "1111011", 9 }
            }
        );

        public static readonly ReadOnlyDictionary<char, string> DIGITS_STRING_MAPPING = new ReadOnlyDictionary<char, string>(new Dictionary<char, string>()
            {
                { '0', "1110111" },
                { '1', "0010010" },
                { '2', "1011101" },
                { '3', "1011011" },
                { '4', "0111010" },
                { '5', "1101011" },
                { '6', "1101111" },
                { '7', "1010010" },
                { '8', "1111111" },
                { '9', "1111011" }
            }
        );

        public MyDb()
        {
            int initialCapacity = 101;
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            Data = new ConcurrentDictionary<Guid, object>(concurrencyLevel, initialCapacity);
        }

        public bool IsSet(Guid key)
        {
            return Data.TryGetValue(key, out var data) && data != null;
        }

        public T Get<T>(Guid key)
        {
            if (Data.TryGetValue(key, out var data))
                return (T)data;

            return default(T);
        }

        public async Task Set<T>(Guid key, T data)
        {
            await Task.Run(() =>
            {
                Data[key] = data;
            });
        }

        public async Task Remove(Guid key)
        {
            await Task.Run(() =>
            {
                object obj;
                Data.TryRemove(key, out obj);
            });
        }

        public async Task Dispose()
        {
            await Task.Run(() =>
            {
                if (Data != null)
                Data = null;
            });
        }

        public async Task<string> SaveToFile()
        {
            try { 
                FileStream fileStream = new FileStream("TrafficLightData.txt", FileMode.Create);
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    foreach (var entry in Data)
                    {
                        await writer.WriteLineAsync(entry.Key.ToString());
                        var data = (SeqData)entry.Value;
                        await writer.WriteLineAsync(string.Join(",", data.StartNumbers));
                        await writer.WriteLineAsync(string.Join(",", data.CurrentNumbers));
                        await writer.WriteLineAsync(string.Join(",", data.BadSectionsStatuses.Select(x => x.ToString())));
                        for(int i = 0; i < data.BadSectionsStartNumbers.Count; i++)
                            await writer.WriteLineAsync(string.Join(",", data.BadSectionsStartNumbers[i].Select(x => x.ToString())));
                        await writer.WriteLineAsync(data.LastColor);
                        await writer.WriteLineAsync(data.LastObservation.color);
                        await writer.WriteLineAsync(string.Join(",", data.LastObservation.numbers));
                    }                        
                }
                return "ok";
            }
            catch (Exception e)
            {
                return e.Message + ": " + e.InnerException;
            }
        }

        public async Task<string> LoadFromFile()
        {
            try
            {
                if (File.Exists("TrafficLightData.txt")) { 
                    FileStream fileStream = new FileStream("TrafficLightData.txt", FileMode.Open);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string line = await reader.ReadLineAsync();
                        Guid sequence = Guid.Parse(line);

                        SeqData data = new SeqData();

                        line = await reader.ReadLineAsync();
                        data.StartNumbers.AddRange(line.Split(",").Select(Int32.Parse).ToList());

                        line = await reader.ReadLineAsync();
                        data.CurrentNumbers.AddRange(line.Split(",").Select(Int32.Parse).ToList());

                        line = await reader.ReadLineAsync();
                        data.BadSectionsStatuses.AddRange(line.Split(",").Select(x => new StringBuilder(x)));

                        for (int i = 0; i < data.StartNumbers.Count; i++) {
                            line = await reader.ReadLineAsync();
                            data.BadSectionsStartNumbers.Add(line.Split(",").Select(x => new StringBuilder(x)).ToList());
                        }

                        line = await reader.ReadLineAsync();
                        data.LastColor = line;

                        line = await reader.ReadLineAsync();
                        data.LastObservation.color = line;

                        line = await reader.ReadLineAsync();
                        data.LastObservation.numbers.AddRange(line.Split(","));

                        await this.Set(sequence, data);

                    }
                    return "ok";
                }
                else
                {
                    return "ok";
                }
            }
            catch(Exception e)
            {
                return e.Message + ": " + e.InnerException;
            }
        }
    }
}
