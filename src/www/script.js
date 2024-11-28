// Base API URL
const apiUrl = "http://localhost:7071/api";

// Funktion för att hämta växelkurser
async function fetchExchangeRates() {
    try {
        const response = await fetch(`${apiUrl}/FetchExchangeRates`, {
            method: "GET",
            headers: {
                "x-functions-key": "din-funktion-nyckel-här", // Ersätt detta med din backend-nyckel
            },
        });

        if (!response.ok) {
            throw new Error(`Error fetching exchange rates: ${response.status}`);
        }

        const data = await response.json();
        displayExchangeRates(data.rates);
    } catch (error) {
        console.error("Error fetching exchange rates:", error);
        alert("Kunde inte hämta växelkurser. Kontrollera anslutningen eller API-konfigurationen.");
    }
}

// Funktion för att visa växelkurser i HTML
function displayExchangeRates(rates) {
    const container = document.getElementById("exchangeRatesContainer");
    container.innerHTML = "<h2>Aktuella växelkurser:</h2>";

    for (const [currency, rate] of Object.entries(rates)) {
        const rateElement = document.createElement("p");
        rateElement.textContent = `${currency}: ${rate.toFixed(2)}`;
        container.appendChild(rateElement);
    }
}

// Funktion för att konvertera valutor
async function convertCurrency(baseCurrency, targetCurrency, amount) {
    try {
        const response = await fetch(`${apiUrl}/ConvertCurrency`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "x-functions-key": "din-funktion-nyckel-här", // Ersätt detta med din backend-nyckel
            },
            body: JSON.stringify({
                baseCurrency,
                targetCurrency,
                amount,
            }),
        });

        if (!response.ok) {
            throw new Error(`Error converting currency: ${response.status}`);
        }

        const result = await response.json();
        displayConversionResult(result);
    } catch (error) {
        console.error("Error converting currency:", error);
        alert("Kunde inte konvertera valuta. Kontrollera att du har angett rätt data och att API:t är tillgängligt.");
    }
}

// Funktion för att visa omvandlingsresultat
function displayConversionResult(result) {
    const resultContainer = document.getElementById("result");
    resultContainer.innerHTML = `
        <h3>Omvandlingsresultat:</h3>
        <p>${result.originalAmount.toFixed(2)} ${result.baseCurrency} är ${result.convertedAmount.toFixed(2)} ${result.targetCurrency}.</p>
    `;
}

// Eventlyssnare för formuläret
document.getElementById("currencyForm").addEventListener("submit", (event) => {
    event.preventDefault();
    const baseCurrency = document.getElementById("baseCurrency").value.toUpperCase();
    const targetCurrency = document.getElementById("targetCurrency").value.toUpperCase();
    const amount = parseFloat(document.getElementById("amount").value);

    if (isNaN(amount) || amount <= 0) {
        alert("Ange ett giltigt belopp.");
        return;
    }

    convertCurrency(baseCurrency, targetCurrency, amount);
});

// Hämta växelkurser vid sidladdning
fetchExchangeRates();
