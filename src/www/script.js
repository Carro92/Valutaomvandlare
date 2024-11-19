// URL till API:et
const apiUrl = "http://localhost:7071/api/FetchExchangeRates";

// Funktion för att hämta växelkurser
async function fetchExchangeRates() {
    try {
        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
            mode: "cors" // Aktivera CORS
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        displayExchangeRates(data.rates); // Skicka 'rates' till display-funktionen
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

    container.innerHTML = ""; // Rensa eventuell tidigare data
    container.appendChild(list);
}

// Anropa funktionen när sidan laddas
window.onload = fetchExchangeRates;
