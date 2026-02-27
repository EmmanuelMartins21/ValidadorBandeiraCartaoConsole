using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ValidadorBandeiraCartaoConsole
{
    public enum CardBrand
    {
        Unknown,
        Visa,
        Mastercard,
        AmericanExpress,
        Discover,
        Diners,
        JCB,
        Elo,
        Hipercard,
        Aura
    }

    public class CreditCard
    {
        public string CardNumber { get; set; }
        public CardBrand Brand { get; set; }
        public bool IsValid { get; set; }
    }

    public class CardBrandPattern
    {
        public CardBrand Brand { get; set; }
        public string Pattern { get; set; }
        public string DisplayName { get; set; }
        public int[] AllowedLengths { get; set; }

        public CardBrandPattern(CardBrand brand, string pattern, string displayName, int[] allowedLengths)
        {
            Brand = brand;
            Pattern = pattern;
            DisplayName = displayName;
            AllowedLengths = allowedLengths;
        }
    }

    public class CreditCardValidator
    {
        public static readonly Dictionary<CardBrand, CardBrandPattern> BrandPatterns = new()
        {
            { CardBrand.Visa, new CardBrandPattern(CardBrand.Visa, "^4[0-9]{12}(?:[0-9]{3})?$", "Visa", new[] { 13, 16, 19 }) },
            { CardBrand.Mastercard, new CardBrandPattern(CardBrand.Mastercard, "^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$", "Mastercard", new[] { 16 }) },
            { CardBrand.AmericanExpress, new CardBrandPattern(CardBrand.AmericanExpress, "^3[47][0-9]{13}$", "American Express", new[] { 15 }) },
            { CardBrand.Discover, new CardBrandPattern(CardBrand.Discover, "^6(?:011|5[0-9]{2})[0-9]{12}$", "Discover", new[] { 16 }) },
            { CardBrand.Diners, new CardBrandPattern(CardBrand.Diners, "^3(?:0[0-5]|[68][0-9])[0-9]{11}$", "Diners Club", new[] { 14 }) },
            { CardBrand.JCB, new CardBrandPattern(CardBrand.JCB, "^(?:2131|1800|35\\d{3})\\d{11}$", "JCB", new[] { 15, 16 }) },
            { CardBrand.Elo, new CardBrandPattern(CardBrand.Elo, "^4011(78|79)|43(1274|8935)|45(0(233|331)|763)|50(4175|6699|67[0-7][0-9]|9000)|627780|63(6297|6368)|650(450|481)|6504(0[0-9]|1[1-9])|6504(2[0-9]|3[0-8])|65048[013]|6505(0[0-9]|1[0-9]|2[0-9]|3[0-8])|6505(4([1-7])|8)|6507(0[0-9]|1[0-9])|6509(0|1|2)|65090([0-2])|6509(2[6-9]|3[0-5])|6515(0|1)|6516(5[2]|6)|6550(0|1)|6[567]", "Elo", new[] { 16 }) },
            { CardBrand.Hipercard, new CardBrandPattern(CardBrand.Hipercard, "^(384100|384140|384160)\\d{12}$", "Hipercard", new[] { 16 }) },
            { CardBrand.Aura, new CardBrandPattern(CardBrand.Aura, "^50(4175|6699|67([0-7][0-9]|8[0-9])|9([012][0-9]|3[0-8]))\\d{10}$", "Aura", new[] { 16 }) }
        };

        /// <summary>
        /// Valida um número de cartão usando o algoritmo de Luhn
        /// </summary>
        public static bool ValidateCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            if (!cardNumber.All(char.IsDigit))
                return false;

            int sum = 0;
            bool isEvenPosition = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';

                if (isEvenPosition)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                isEvenPosition = !isEvenPosition;
            }

            return sum % 10 == 0;
        }

        /// <summary>
        /// Identifica a bandeira do cartão
        /// </summary>
        public static CardBrand IdentifyBrand(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return CardBrand.Unknown;

            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            foreach (var pattern in BrandPatterns.Values)
            {
                if (Regex.IsMatch(cardNumber, pattern.Pattern))
                    return pattern.Brand;
            }

            return CardBrand.Unknown;
        }

        /// <summary>
        /// Valida um cartão de crédito completo
        /// </summary>
        public static CreditCard ValidateCard(string cardNumber)
        {
            var cleanCardNumber = cardNumber?.Replace(" ", "").Replace("-", "") ?? string.Empty;

            var card = new CreditCard
            {
                CardNumber = cleanCardNumber,
                Brand = IdentifyBrand(cleanCardNumber),
                IsValid = false
            };

            if (card.Brand == CardBrand.Unknown)
                return card;

            if (!ValidateCardNumber(cleanCardNumber))
                return card;

            var brandPattern = BrandPatterns[card.Brand];
            if (!brandPattern.AllowedLengths.Contains(cleanCardNumber.Length))
                return card;

            card.IsValid = true;
            return card;
        }

        /// <summary>
        /// Adiciona uma nova bandeira (permite extensibilidade)
        /// </summary>
        public static void AddCardBrand(CardBrand brand, string pattern, string displayName, int[] allowedLengths)
        {
            BrandPatterns[brand] = new CardBrandPattern(brand, pattern, displayName, allowedLengths);
        }

        /// <summary>
        /// Obtém informações sobre uma bandeira
        /// </summary>
        public static CardBrandPattern GetBrandPattern(CardBrand brand)
        {
            return BrandPatterns.TryGetValue(brand, out var pattern) ? pattern : null;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== VALIDADOR DE CARTÃO DE CRÉDITO ===\n");

            string[] testCards = new[]
            {
                "4532015112830366",      // Visa (16 dígitos)
                "5425233010103442",      // Mastercard (16 dígitos)
                "378282246310005",       // American Express (15 dígitos)
                "6011111111111117",      // Discover (16 dígitos)
                "3530111333300000",      // JCB (16 dígitos)
                "4111111111111111",      // Visa inválido (Luhn)
            };

            foreach (var cardNumber in testCards)
            {
                var card = CreditCardValidator.ValidateCard(cardNumber);
                var brandInfo = CreditCardValidator.GetBrandPattern(card.Brand);
                var brandDisplayName = brandInfo?.DisplayName ?? "Desconhecida";

                Console.WriteLine($"Cartão: {cardNumber}");
                Console.WriteLine($"Bandeira: {card.Brand} ({brandDisplayName})");
                Console.WriteLine($"Válido: {card.IsValid}");
                Console.WriteLine($"Dígitos: {card.CardNumber.Length}\n");
            }

            Console.WriteLine("=== ADICIONANDO NOVA BANDEIRA ===\n");
            CreditCardValidator.AddCardBrand(
                brand: CardBrand.Unknown,
                pattern: "^9[0-9]{15}$",
                displayName: "Minha Bandeira Futura",
                allowedLengths: new[] { 16, 19 }
            );

            Console.WriteLine("Nova bandeira adicionada com sucesso!");
        }
    }
}