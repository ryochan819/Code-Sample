using UnityEngine;

public static class CurrencySetting
{
    public static Currency gameCurrency = Currency.JPY;
}

public enum Currency
{
    JPY,
    USD,
    CN,
    GBP,
    EURO
}