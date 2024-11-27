const isLocal = window.location.hostname === "127.0.0.1" || window.location.hostname === "localhost";

// Kontrollera bas-URL
const baseApiUrl = isLocal
    ? "http://localhost:7071/api/" // Lokal utveckling
    : "https://valutaomvandlare-functionapp.azurewebsites.net/api/"; // Publicerad app

console.log(`Base API URL is set to: ${baseApiUrl}`);

// Funktion för att testa om API fungerar
async function testApi() {
    try {
        const response = await fetch(`${baseApiUrl}FetchExchangeRates`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });
        if (!response.ok) throw new Error(`API test failed: ${response.status}`);
        console.log("API test successful.");
    } catch (error) {
        console.error("API test failed:", error);
        alert("Kunde inte ansluta till API. Kontrollera URL och publicering.");
    }
}

// Testa API när sidan laddas
window.onload = async () => {
    console.log("Testing API connection...");
    await testApi();
};
