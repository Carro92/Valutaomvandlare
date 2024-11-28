// Kontrollera om vi kör lokalt eller på Azure
const isLocal = window.location.hostname === "127.0.0.1" || window.location.hostname === "localhost";
const apiUrl = isLocal ? "http://localhost:7071/api" : "https://valutaomvandlare-functionapp.azurewebsites.net/api"; // Din Azure-URL

// Funktion för att hämta växelkurser
async function fetchExchangeRates() {
    try {
        const response = await fetch(`${apiUrl}/FetchExchangeRates`, {
            method: "GET",
        });

        if (!response.ok) {
            throw new Error(`Error fetching exchange rates: ${response.status}`);
        }

        const data = await response.json();
        displayExchangeRates(data.rates);
    } catch (error) {
        console.error("Error fetching exchange rates:", error);
    }
}

// Funktion för att visa växelkurser i HTML
function displayExchangeRates(rates) {
    const container = document.getElementById("exchangeRatesContainer");
    container.innerHTML = "<h2>Aktuella växelkurser:</h2>";

    for (const [currency, rate] of Object.entries(rates)) {
        const rateElement = document.createElement("p");
        rateElement.textContent = `${currency}: ${rate}`;
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
    }
}

// Funktion för att visa omvandlingsresultat
function displayConversionResult(result) {
    const resultContainer = document.getElementById("result");
    resultContainer.innerHTML = `
        <h3>Omvandlingsresultat:</h3>
        <p>${result.originalAmount} ${result.baseCurrency} är ${result.convertedAmount} ${result.targetCurrency}.</p>
    `;
}

// Eventlyssnare för formuläret
document.getElementById("currencyForm").addEventListener("submit", (event) => {
    event.preventDefault();
    const baseCurrency = document.getElementById("baseCurrency").value.toUpperCase();
    const targetCurrency = document.getElementById("targetCurrency").value.toUpperCase();
    const amount = parseFloat(document.getElementById("amount").value);

    convertCurrency(baseCurrency, targetCurrency, amount);
});

// Hämta växelkurser vid sidladdning
fetchExchangeRates();
