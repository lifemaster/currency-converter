using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace CurrencyConverter
{
    [DataContract]
    class CurrencyExchange
    {
        [DataMember]
        public int r030;

        [DataMember]
        public string txt;

        [DataMember]
        public decimal rate;

        [DataMember]
        public string cc;

        [DataMember]
        public string exchangedate;
    }

    enum CurrencyCode
    {
        USD,
        RUB,
        EUR
    }

    class Program
    {
        private static readonly string getDollarRateUrl = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange";

        private static readonly HttpClient httpClient = new HttpClient();

        static void PrintGreating()
        {
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("ДОБРО ПОЖАЛОВАТЬ В КОНВЕРТЕР ВАЛЮТ! (КУРС ПО НБУ)");
            Console.WriteLine("-------------------------------------------------");
            //Console.WriteLine();
            //Console.WriteLine("Для выхода нажмите Ctrl + C или закройте окно");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.Title = "Конвертер валют";

            PrintGreating();
            RequestUserInput();
        }

        static void RequestUserInput()
        {
            string currency = GetCurrencyFromUserInput();
            DateTime date;

            do
            {
                date = GetDateFromUserInput();
            }
            while (!DateValidate(date, currency));

            Task<Decimal> rateResult = GetDollarRate(date.ToString("yyyyMMdd"), currency);

            Console.WriteLine();

            try
            {
                Console.WriteLine("----------------------------------------------------------------------");
                Console.WriteLine();
                Console.WriteLine($"Курс валюты {currency} на дату {date.ToString("dd.MM.yyyy")} равен {rateResult.Result} грн");
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------------------------------");
            }
            catch
            {
                Console.WriteLine($"Нет данных в ответе от сервера. Попробуйте изменить параметры запроса.");
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------------------------------");
            }
            finally
            {
                Console.WriteLine();

                RequestUserInput();
            }
        }

        async static private Task<decimal> GetDollarRate(string date, string currencyCode)
        {
            var serverResponse = await httpClient.GetStringAsync(getDollarRateUrl + "?date=" + date + "&json&valcode=" + currencyCode);
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serverResponse));
            var serializer = new DataContractJsonSerializer(typeof(CurrencyExchange[]));

            var obj = serializer.ReadObject(memoryStream) as CurrencyExchange[];

            memoryStream.Close();

            return obj[0].rate;
        }

        static DateTime GetDateFromUserInput()
        {
            bool isNotValidInput = true;
            DateTime date;

            do
            {
                Console.WriteLine();
                Console.Write("Введите дату (форматы: ДД.ММ.ГГГГ, ДД/ММ/ГГГГ, ГГГГ-ММ-ДД): ");

                var userValue = Console.ReadLine();

                if (!String.IsNullOrEmpty(userValue))
                {
                    userValue = userValue.Trim();
                }

                if (DateTime.TryParse(userValue, out date))
                {
                    isNotValidInput = false;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Неверный формат даты. Повторите еще раз.");
                }
            }
            while (isNotValidInput);

            return date;
        }

        static bool DateValidate(DateTime date, string currency)
        {
            if (date > DateTime.Now)
            {
                Console.WriteLine();
                Console.WriteLine($"Вы выбрали дату из будущего. Повторите ввод даты.");
                return false;
            }
            else if (date < new DateTime(1996, 9, 2))
            {
                Console.WriteLine();
                Console.WriteLine($"Гривна была введена в оборот 2 сентября 1996 года. На дату {date.ToString("dd.MM.yyyy")} гривна еще не существовала");
                return false;
            }
            else if (date < new DateTime(1999, 1, 1) && currency == CurrencyCode.EUR.ToString())
            {
                Console.WriteLine();
                Console.WriteLine($"Евро была введена в оборот 1 января 1999 года. На дату {date.ToString("dd.MM.yyyy")} евро еще не существовала");
                return false;
            }
            else
            {
                return true;
            }
        }

        static string GetCurrencyFromUserInput()
        {
            bool isNotValidInput = true;
            string currency = "";

            do
            {
                Console.Write("Введите код валюты (USD/RUB/EUR) или Ctrl + C для выхода: ");
                string userValue = Console.ReadLine();

                if (!String.IsNullOrEmpty(userValue))
                {
                    userValue = userValue.Trim().ToUpper();
                }

                if (!String.IsNullOrEmpty(userValue) && Enum.IsDefined(typeof(CurrencyCode), userValue))
                {
                    isNotValidInput = false;
                    currency = userValue;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Неверная валюта. Повторите ввод.");
                    Console.WriteLine();
                }
            }
            while (isNotValidInput);

            return currency;
        }
    }
}
