// URL till API:et
const apiUrl = "http://127.0.0.1:7071/api/FetchExchangeRates"; // För localhost backend



// Funktion för att hämta växelkurser
async function fetchExchangeRates() {
    console.log("Start fetching exchange rates..."); // Debug-logg

    try {
        console.log("Sending request to API:", apiUrl); // Debug-logg
        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Content-Type": "application/json"
            },
        });

        console.log("Response received:", response); // Debug-logg

        if (!response.ok) {
            console.error("API response is not OK. Status:", response.status); // Debug-logg
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log("Data fetched successfully:", data); // Debug-logg
        displayExchangeRates(data.rates); // Skicka 'rates' till display-funktionen
    } catch (error) {
        console.error("Error fetching exchange rates:", error); // Debug-logg
    }

    console.log("Finished fetching exchange rates."); // Debug-logg
}

// Funktion för att visa växelkurser i HTML
function displayExchangeRates(rates) {
    console.log("Displaying exchange rates:", rates); // Debug-logg

    const container = document.getElementById("exchangeRatesContainer");
    const list = document.createElement("ul");

    for (const [currency, rate] of Object.entries(rates)) {
        console.log(`Adding rate to list: ${currency} - ${rate}`); // Debug-logg
        const listItem = document.createElement("li");
        listItem.textContent = `${currency}: ${rate.toFixed(2)}`;
        list.appendChild(listItem);
    }

    container.innerHTML = ""; // Rensa eventuell tidigare data
    container.appendChild(list);

    console.log("Exchange rates displayed successfully."); // Debug-logg
}

// Anropa funktionen när sidan laddas
window.onload = () => {
    console.log("Page loaded. Fetching exchange rates..."); // Debug-logg
    fetchExchangeRates();
};
