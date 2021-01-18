using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrafficLight.Models;

namespace TrafficLight.Controllers
{
    public class ObservationController : ControllerBase
    {
        private MyDb _db;
        public static int N = 2; //два разряда
        public static int Ns = 7; //количество секций

        public ObservationController(MyDb db)
        {
            _db = db;
        }

        // POST observation/add
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ObservationWithSequence obseq)
        {
            ObjectResult Result = null;

            await Task.Run(() =>
            {
                if (_db.IsSet(obseq.sequence)) { 

                    var data = _db.Get<SeqData>(obseq.sequence);

                    if(data.LastColor == "green") {

                        if (obseq.observation.color == "red")
                        { 
                            data.LastColor = "red";
                            //значит предыдущее знчение было 1, ищем соответствующее StartNumber, остальные удаляем, определяем неработающие секции, т.к. знаем точно, какие числа были ранее
                            if (data.CurrentNumbers.Contains(1))
                            {
                                for (int k = data.CurrentNumbers.Count - 1; k >= 0; k--)
                                {
                                    if (data.CurrentNumbers[k] != 1)
                                    {
                                        data.StartNumbers.RemoveAt(k);
                                        data.BadSectionsStartNumbers.RemoveAt(k);
                                    }

                                }
                                data.CurrentNumbers.RemoveAll(x => x != 1);

                                string sn = "[" + data.StartNumbers[0] + "]";
                                string bs = "[" + string.Join(",", data.BadSectionsStartNumbers[0]) + "]";

                                Result = new ObjectResult(new
                                {
                                    status = "ok",
                                    response = new
                                    {
                                        start = sn,
                                        missing = bs
                                    }
                                })
                                { StatusCode = 200 };

                            }
                            else
                            {
                                //не найдено 1 ни в одном текущем числе
                                Result = new ObjectResult(new
                                {
                                    status = "error",
                                    msg = "No solutions found"
                                })
                                { StatusCode = 422 };
                            }
                        }
                        else if (obseq.observation.color != "green")
                        {
                            //неизвестный цвет
                            Result = new ObjectResult(new
                            {
                                status = "error",
                                msg = "Unknown color"
                            })
                            { StatusCode = 422 };
                        }
                        else
                        {
                            if(obseq.observation.numbers.Count == N && obseq.observation.numbers[0].Length == Ns && obseq.observation.numbers[1].Length == Ns) { 
                                if(data.LastObservation.color != obseq.observation.color || data.LastObservation.numbers[0] != obseq.observation.numbers[0] 
                                    || data.LastObservation.numbers[1] != obseq.observation.numbers[1])
                                {
                                    if (data.CurrentNumbers.Count > 0)
                                        for (int i = 0; i < data.CurrentNumbers.Count; i++)
                                            data.CurrentNumbers[i] = data.CurrentNumbers[i] - 1; // текущий счетчик уменьшается на 1

                                    //поиск возможных цифр в каждом разряде
                                    var digit = new List<List<int>>();

                                    for (int j = 0; j < N; j++)
                                        digit.Add(MyDb.STRING_DIGITS_MAPPING.Where(
                                                  x => x.Key.Like(obseq.observation.numbers[j].Replace("0", "_"))
                                                  && ((data.CurrentNumbers.Count == 0) 
                                                        || data.CurrentNumbers.Select(y => int.Parse(y.ToString().PadLeft(2, '0')[j].ToString())).ToList().Contains(x.Value))
                                            ).Select(x => x.Value).ToList());                              

                                    //возможные числа, которые пришли
                                    var possibleNumbers = digit[0].SelectMany(x => digit[1].Select(y => x * 10 + y)).ToList();

                                    if (data.StartNumbers.Count == 0)
                                    {
                                        data.StartNumbers.AddRange(possibleNumbers);
                                        data.CurrentNumbers.AddRange(possibleNumbers);
                                        for (int i = 0; i < data.StartNumbers.Count; i++)
                                            data.BadSectionsStartNumbers.Add(new List<StringBuilder>() { new StringBuilder("0000000"), new StringBuilder("0000000") });
                                    }
                                    else
                                    {
                                        possibleNumbers = possibleNumbers.Where(x => data.CurrentNumbers.Contains(x)).ToList();

                                        var CurForDel = data.CurrentNumbers.Where(x => !possibleNumbers.Contains(x)).ToList();
                                        foreach (var c in CurForDel) { //очищаем те деревья, которые не подходят для присланных данных
                                            int k = data.CurrentNumbers.IndexOf(c);
                                            data.CurrentNumbers.RemoveAt(k);
                                            data.StartNumbers.RemoveAt(k);
                                            data.BadSectionsStartNumbers.RemoveAt(k);
                                        }
                                    }

                                    if (data.StartNumbers.Count == 0)
                                    {
                                        //по полученным данным стартовое число невозможно найти
                                        Result = new ObjectResult(new
                                        {
                                            status = "error",
                                            msg = "No solutions found"
                                        })
                                        { StatusCode = 422 };
                                    }
                                    else
                                    {
                                        //поиск нерабочих секций
                                        for (int j = 0; j < N; j++)
                                        {
                                            var strdigits = MyDb.STRING_DIGITS_MAPPING.Where(x => digit[j].Contains(x.Value)).Select(x => x.Key).ToList();

                                            if (!strdigits.Contains(obseq.observation.numbers[j]))//если пришедшие данные не подходят ни одной возможной цифре, то есть нерабоча секция
                                            {
                                                for (int i = 0; i < Ns; i++)
                                                {
                                                    string s1 = obseq.observation.numbers[j];

                                                    bool bad_section = true;

                                                    foreach (var s in strdigits)
                                                        if (s1[i] == '0')//если секция не работает в пришедших данных,
                                                        {
                                                            if (s[i] == '0')//если в возмоной цифре секция не работает
                                                            {
                                                                bad_section = false;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            bad_section = false;
                                                            break;
                                                        }

                                                    if (bad_section)
                                                        if (data.BadSectionsStatuses[j][i] == ' ')
                                                            data.BadSectionsStatuses[j][i] = '1';
                                                        else if (data.BadSectionsStatuses[j][i] == '0')
                                                            //нерабочая секция вдруг заработала
                                                            Result = new ObjectResult(new
                                                            {
                                                                status = "error",
                                                                msg = "No solutions found"
                                                            })
                                                            { StatusCode = 422 };

                                                }
                                            }

                                            //если секция показывала, а теперь нет, то нужно убрать все дерево                                
                                            for (int i = 0; i < Ns; i++)
                                            {
                                                if (data.BadSectionsStatuses[j][i] == '0' && obseq.observation.numbers[j][i] == '0')
                                                    foreach (string s in strdigits)
                                                        if (s[i] == '1' && data.CurrentNumbers.Where(x => int.Parse(x.ToString().PadLeft(2, '0')[j].ToString()) == MyDb.STRING_DIGITS_MAPPING[s])
                                                                                                    .ToList().Count > 0)
                                                        {
                                                            foreach (var cn in data.CurrentNumbers.Where(x => int.Parse(x.ToString().PadLeft(2, '0')[j].ToString()) == MyDb.STRING_DIGITS_MAPPING[s])
                                                                                                    .ToList())
                                                            {
                                                                int k = data.CurrentNumbers.IndexOf(cn);
                                                                data.StartNumbers.RemoveAt(k);
                                                                data.CurrentNumbers.RemoveAt(k);
                                                                data.BadSectionsStartNumbers.RemoveAt(k);
                                                            }
                                                        }
                                                if (obseq.observation.numbers[j][i] == '1')
                                                {
                                                    if (data.BadSectionsStatuses[j][i] == ' ')
                                                        data.BadSectionsStatuses[j][i] = '0';//рабочая секция
                                                    else if (data.BadSectionsStatuses[j][i] == '1')
                                                        //ошибка - нерабочая секция заработала
                                                        Result = new ObjectResult(new
                                                        {
                                                            status = "error",
                                                            msg = "No solutions found"
                                                        })
                                                        { StatusCode = 422 };
                                                }

                                            }

                                            //сохраняем возможные нерабочие секции, которые станут полезными, когда выяснится стартовое число
                                            for (int k = 0; k < data.CurrentNumbers.Count; k++)
                                                if (obseq.observation.numbers[j] != MyDb.DIGITS_STRING_MAPPING[data.CurrentNumbers[k].ToString().PadLeft(2, '0')[j]])
                                                    for (int i = 0; i < Ns; i++)
                                                        if (obseq.observation.numbers[j][i] == '0' 
                                                                    && MyDb.DIGITS_STRING_MAPPING[data.CurrentNumbers[k].ToString().PadLeft(2, '0')[j]][i] == '1' 
                                                                    && data.BadSectionsStartNumbers[k][j][i] == '0')
                                                            data.BadSectionsStartNumbers[k][j][i] = '1';
                                        }
                                    }

                                    //сохраняем полученные данные
                                    data.LastObservation = obseq.observation;
                                }
                                else
                                {   //значения отправлены дважды
                                    Result = new ObjectResult(new
                                    {
                                        status = "error",
                                        msg = "No solutions found"
                                    })
                                    { StatusCode = 422 };
                                }
                            }
                            else
                            {
                                Result = new ObjectResult(new
                                {
                                    status = "error",
                                    msg = "Format error"
                                })
                                { StatusCode = 400 };
                            }
                        }
                    
                        if(Result == null) { 

                            if (data.StartNumbers.Count == 0)
                            {
                                Result = new ObjectResult(new
                                {
                                    status = "error",
                                    msg = "No solutions found"
                                })
                                { StatusCode = 422 };
                            }
                            else
                            {
                                if(data.CurrentNumbers.Count == 1)//если начальное число найдено, то можно точно сказать какое сейчас число и определить нерабочие секции
                                {
                                    for (int j = 0; j < N; j++)
                                        if (obseq.observation.numbers[j] != MyDb.DIGITS_STRING_MAPPING[data.CurrentNumbers[0].ToString().PadLeft(2, '0')[j]])
                                            for(int i = 0; i < N; i++)
                                                if(obseq.observation.numbers[j][i] == '0' && MyDb.DIGITS_STRING_MAPPING[data.CurrentNumbers[0].ToString().PadLeft(2, '0')[j]][i] == '1')
                                                    data.BadSectionsStatuses[j][i] = '1';
                                }

                                string sn = "[" + string.Join(",", data.StartNumbers) + "]";
                                string bs = "[" + string.Join(",", data.BadSectionsStatuses).Replace(' ', '0') + "]";

                                Result = new ObjectResult(new
                                {
                                    status = "ok",
                                    response = new { start = sn,
                                        missing = bs
                                    }
                                })
                                { StatusCode = 200 };
                            }
                        }
                    }
                    else
                    {
                        if (data.StartNumbers.Count == 0)
                            Result = new ObjectResult(new
                            {
                                status = "error",
                                msg = "There isn't enough data"
                            })
                            { StatusCode = 422 };
                        else
                            Result = new ObjectResult(new
                            {
                                status = "error",
                                msg = "The red observation should be the last"
                            })
                            { StatusCode = 422 };
                    }
                }
                else
                {
                    Result = new ObjectResult(new
                    {
                        status = "error",
                        msg = "The sequence isn't found"
                    })
                    { StatusCode = 404 };
                }
            });

            return Result;
        }


        // POST observation/save
        [HttpPost]
        public async Task<IActionResult> Save()
        {
            string res = await _db.SaveToFile();
            if (res == "ok")
            {

                return new ObjectResult(new
                {
                    status = "ok",
                    response = "Export to file is done"
                })
                { StatusCode = 200 };
            }
            else
            {
                return new ObjectResult(new
                {
                    status = "error",
                    msg = res
                })
                { StatusCode = 500 };
            }
        }


        // POST observation/load
        [HttpPost]
        public async Task<IActionResult> Load()
        {
            string res = await _db.LoadFromFile();
            if (res == "ok")
            {

                return new ObjectResult(new
                {
                    status = "ok",
                    response = "Load from file is done"
                })
                { StatusCode = 200 };
            }
            else
            {
                return new ObjectResult(new
                {
                    status = "error",
                    msg = res
                })
                { StatusCode = 404 };
            }
        }
    }
}