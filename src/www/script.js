// Kontrollera var appen körs (lokalt eller i Azure)
const isLocal = window.location.hostname === "127.0.0.1" || window.location.hostname === "localhost";

// Sätt API URL baserat på miljö
const baseApiUrl = isLocal
    ? "http://localhost:7071/api" // Lokal URL
    : "https://valutaomvandlare-functionapp.azurewebsites.net/api"; // Azure URL

const fetchExchangeRatesUrl = `${baseApiUrl}/FetchExchangeRates`; // URL för att hämta växelkurser
const storeConversionHistoryUrl = `${baseApiUrl}/StoreConversionHistory`; // URL för att lagra konverteringshistorik

console.log(`Base API URL is set to: ${baseApiUrl}`);

// Funktion för att hämta växelkurser (GET)
async function fetchExchangeRates() {
    console.log("Start fetching exchange rates...");
    try {
        const response = await fetch(fetchExchangeRatesUrl, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            console.error("API response is not OK. Status:", response.status);
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log("Data fetched successfully:", data);
        displayExchangeRates(data.rates);
    } catch (error) {
        console.error("Error fetching exchange rates:", error);
    }
}

// Funktion för att visa växelkurser i HTML
function displayExchangeRates(rates) {
    const container = document.getElementById("exchangeRatesContainer");
    const list = document.createElement("ul");

    for (const [currency, rate] of Object.entries(rates)) {
        const listItem = document.createElement("li");
        listItem.textContent = `${currency}: ${rate.toFixed(2)}`;
        list.appendChild(listItem);
    }

    container.innerHTML = "";
    container.appendChild(list);
}

// Lägg till händelsehanterare för formuläret (POST)
document.getElementById("currencyForm").addEventListener("submit", async function (event) {
    event.preventDefault();

    const formData = {
        baseCurrency: document.getElementById("baseCurrency").value,
        targetCurrency: document.getElementById("targetCurrency").value,
        amount: document.getElementById("amount").value
    };

    try {
        const response = await fetch(storeConversionHistoryUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(formData)
        });

        if (!response.ok) {
            console.error("API response is not OK. Status:", response.status);
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log("Data from POST request:", data);

        document.getElementById("result").innerText = `${formData.amount} ${formData.baseCurrency} = ${data.convertedAmount.toFixed(2)} ${formData.targetCurrency}`;
    } catch (error) {
        console.error("Error:", error);
    }
});

// Hämta valutakurser när sidan laddas
window.onload = () => {
    console.log("Page loaded. Fetching exchange rates...");
    fetchExchangeRates();
};
