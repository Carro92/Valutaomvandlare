const isLocal = window.location.hostname === "127.0.0.1" || window.location.hostname === "localhost";

const baseApiUrl = isLocal
    ? "http://localhost:7071/api/" // Lokal URL
    : "https://valutaomvandlare-functionapp.azurewebsites.net/api/"; // Azure URL

console.log(`Base API URL is set to: ${baseApiUrl}`);

async function fetchExchangeRates() {
    console.log("Start fetching exchange rates...");
    try {
        const response = await fetch(`${baseApiUrl}FetchExchangeRates`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "x-functions-key": "2RL2SoFlskqGdNAUtz0wfAapPBHSpo6l-qgO5qlgDtwhAzFuAHATWw==" // Lägg in din function key här
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

window.onload = () => {
    console.log("Page loaded. Fetching exchange rates...");
    fetchExchangeRates();
};
