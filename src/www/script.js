// Bas-URL för API:t
const baseUrl = "http://localhost:7071/api/";
const apiKey = localStorage.getItem("FUNCTION_APP_KEY") || "REPLACE_WITH_YOUR_KEY";

// Funktion för att hämta aktuella växelkurser
async function fetchExchangeRates() {
    try {
        const response = await fetch(`${baseUrl}FetchExchangeRates`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "x-functions-key": apiKey,
            },
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        displayExchangeRates(data.rates);
    } catch (error) {
        console.error("Error fetching exchange rates:", error);
        alert("Kunde inte hämta växelkurser. Kontrollera anslutningen.");
    }
}

// Funktion för att visa växelkurser i "exchangeRatesContainer"
function displayExchangeRates(rates) {
    const container = document.getElementById("exchangeRatesContainer");
    container.innerHTML = "<h2>Aktuella växelkurser:</h2>"; // Återställ innehållet
    for (const [currency, rate] of Object.entries(rates)) {
        const rateElement = document.createElement("p");
        rateElement.textContent = `${currency}: ${rate}`;
        container.appendChild(rateElement);
    }
}

// Funktion för att omvandla valuta
async function convertCurrency(event) {
    event.preventDefault(); // Förhindra formulärets standardbeteende

    const baseCurrency = document.getElementById("baseCurrency").value.toUpperCase();
    const targetCurrency = document.getElementById("targetCurrency").value.toUpperCase();
    const amount = parseFloat(document.getElementById("amount").value);

    if (!baseCurrency || !targetCurrency || isNaN(amount)) {
        alert("Fyll i alla fält korrekt.");
        return;
    }

    try {
        const response = await fetch(`${baseUrl}ConvertCurrency`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "x-functions-key": apiKey,
            },
            body: JSON.stringify({
                baseCurrency,
                targetCurrency,
                amount,
            }),
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        displayConversionResult(data);
    } catch (error) {
        console.error("Error converting currency:", error);
        alert("Kunde inte konvertera valutan. Kontrollera anslutningen.");
    }
}

// Funktion för att visa omvandlingsresultat i "result"
function displayConversionResult(data) {
    const resultContainer = document.getElementById("result");
    resultContainer.innerHTML = `
        <h3>Omvandlingsresultat:</h3>
        <p>${data.originalAmount} ${data.baseCurrency} motsvarar ${data.convertedAmount} ${data.targetCurrency}.</p>
    `;
}

// Event Listeners
document.addEventListener("DOMContentLoaded", fetchExchangeRates);
document.getElementById("currencyForm").addEventListener("submit", convertCurrency);
