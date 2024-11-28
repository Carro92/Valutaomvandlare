// API-basadress och endpoints
const apiUrl = "https://valutaomvandlare-functionapp.azurewebsites.net/api";
const fetchExchangeRatesEndpoint = "/FetchExchangeRates";
const convertCurrencyEndpoint = "/ConvertCurrency";

// Hämtar API-nyckeln från en miljövariabel som sätts av backend-konfigurationen
const functionAppKey = process.env.FUNCTION_APP_KEY || "REPLACE_WITH_DEFAULT_IF_NEEDED";

async function fetchExchangeRates() {
    try {
        console.log("Fetching exchange rates...");
        const response = await fetch(`${apiUrl}${fetchExchangeRatesEndpoint}`, {
            method: "GET",
            headers: {
                "x-functions-key": functionAppKey
            },
        });
        if (!response.ok) {
            throw new Error(`Error fetching exchange rates: ${response.statusText}`);
        }
        const data = await response.json();
        console.log("Exchange rates fetched successfully:", data);
        displayExchangeRates(data.rates);
    } catch (error) {
        console.error("Error fetching exchange rates:", error);
        alert("Kunde inte hämta växelkurser. Kontrollera anslutningen.");
    }
}

async function convertCurrency(baseCurrency, targetCurrency, amount) {
    try {
        console.log(`Converting ${amount} ${baseCurrency} to ${targetCurrency}...`);
        const response = await fetch(`${apiUrl}${convertCurrencyEndpoint}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "x-functions-key": functionAppKey
            },
            body: JSON.stringify({
                baseCurrency: baseCurrency,
                targetCurrency: targetCurrency,
                amount: parseFloat(amount),
            }),
        });
        if (!response.ok) {
            throw new Error(`Error converting currency: ${response.statusText}`);
        }
        const data = await response.json();
        console.log("Currency converted successfully:", data);
        displayConversionResult(data);
    } catch (error) {
        console.error("Error converting currency:", error);
        alert("Kunde inte konvertera valuta. Kontrollera anslutningen.");
    }
}

function displayExchangeRates(rates) {
    const ratesContainer = document.getElementById("exchangeRates");
    ratesContainer.innerHTML = ""; // Rensa tidigare resultat
    for (const [currency, rate] of Object.entries(rates)) {
        const rateElement = document.createElement("p");
        rateElement.textContent = `${currency}: ${rate}`;
        ratesContainer.appendChild(rateElement);
    }
}

function displayConversionResult(result) {
    const conversionResult = document.getElementById("conversionResult");
    conversionResult.textContent = `${result.originalAmount} ${result.baseCurrency} motsvarar ${result.convertedAmount} ${result.targetCurrency}`;
}

// Hantera hämta växelkurser-knappen
document.getElementById("fetchRatesBtn").addEventListener("click", () => {
    fetchExchangeRates();
});

// Hantera konverteringsformuläret
document.getElementById("convertForm").addEventListener("submit", (event) => {
    event.preventDefault();
    const baseCurrency = document.getElementById("baseCurrency").value.toUpperCase();
    const targetCurrency = document.getElementById("targetCurrency").value.toUpperCase();
    const amount = document.getElementById("amount").value;
    convertCurrency(baseCurrency, targetCurrency, amount);
});
