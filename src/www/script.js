// Kontrollera var appen körs (lokalt eller i Azure)
const isLocal = window.location.hostname === "127.0.0.1" || window.location.hostname === "localhost";

// Sätt bas-URL baserat på miljö
const baseApiUrl = isLocal
    ? "http://localhost:7071/api/" // Lokal URL
    : "https://valutaomvandlare-functionapp.azurewebsites.net/api/"; // Azure URL

console.log(`Base API URL is set to: ${baseApiUrl}`);

// Funktion för att hämta växelkurser (GET)
async function fetchExchangeRates() {
    console.log("Start fetching exchange rates...");
    try {
        const response = await fetch(`${baseApiUrl}FetchExchangeRates`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "x-functions-key": await getFunctionKey() // Dynamiskt hämta nyckeln
            }
        });

        if (!response.ok) {
            console.error("API response is not OK. Status:", response.status);
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log("Exchange rates fetched successfully:", data);
        displayExchangeRates(data.rates);
    } catch (error) {
        console.error("Error fetching exchange rates:", error);
        alert("Kunde inte hämta växelkurser. Kontrollera anslutningen.");
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

    const baseCurrency = document.getElementById("baseCurrency").value.toUpperCase();
    const targetCurrency = document.getElementById("targetCurrency").value.toUpperCase();
    const amount = parseFloat(document.getElementById("amount").value);

    if (isNaN(amount) || !baseCurrency || !targetCurrency) {
        alert("Vänligen fyll i alla fält korrekt.");
        return;
    }

    try {
        const response = await fetch(`${baseApiUrl}ConvertCurrency`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "x-functions-key": await getFunctionKey() // Dynamiskt hämta nyckeln
            },
            body: JSON.stringify({
                baseCurrency,
                targetCurrency,
                amount
            })
        });

        if (!response.ok) {
            console.error("API response is not OK. Status:", response.status);
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log("Conversion result:", data);

        const resultElement = document.getElementById("result");
        resultElement.innerText = `${amount} ${baseCurrency} = ${data.convertedAmount.toFixed(2)} ${targetCurrency}`;
    } catch (error) {
        console.error("Error:", error);
        alert("Ett fel inträffade vid valutaomvandlingen. Kontrollera anslutningen eller försöka igen senare.");
    }
});

// Funktion för att dynamiskt hämta funktionens nyckel
async function getFunctionKey() {
    if (isLocal) {
        return "DEFAULT_LOCAL_KEY"; // Byt ut detta mot rätt lokal nyckel (finns i `local.settings.json`)
    } else {
        const response = await fetch("/api/getFunctionKey"); // Använd en säker endpoint för att hämta nyckeln i Azure
        if (!response.ok) {
            throw new Error("Misslyckades att hämta Function Key från servern");
        }
        const data = await response.json();
        return data.key; // Returnera nyckeln från API
    }
}

// Hämta växelkurser när sidan laddas
window.onload = () => {
    console.log("Page loaded. Fetching exchange rates...");
    fetchExchangeRates();
};
