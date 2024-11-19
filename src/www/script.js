document.addEventListener("DOMContentLoaded", () => {
    const exchangeRatesContainer = document.getElementById("exchangeRatesContainer");
    const currencyForm = document.getElementById("currencyForm");
    const resultDiv = document.getElementById("result");

    // Funktion för att hämta växelkurser
    async function fetchExchangeRates() {
        try {
            const response = await fetch("http://localhost:7071/api/FetchExchangeRates");
            const data = await response.json();

            // Visa växelkurser i HTML
            const ratesHtml = Object.entries(data.rates).map(([currency, rate]) => `
                <div>${currency}: ${rate}</div>
            `).join("");
            exchangeRatesContainer.innerHTML = `<h2>Aktuella växelkurser</h2>${ratesHtml}`;
        } catch (error) {
            console.error("Kunde inte hämta växelkurser:", error);
        }
    }

    // Funktion för att konvertera valutor
    currencyForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const baseCurrency = document.getElementById("baseCurrency").value;
        const targetCurrency = document.getElementById("targetCurrency").value;
        const amount = parseFloat(document.getElementById("amount").value);

        if (!baseCurrency || !targetCurrency || isNaN(amount)) {
            resultDiv.innerText = "Alla fält måste fyllas i korrekt!";
            return;
        }

        try {
            const response = await fetch("http://localhost:7071/api/StoreConversionHistory", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    baseCurrency: baseCurrency.toUpperCase(),
                    targetCurrency: targetCurrency.toUpperCase(),
                    rate: amount
                })
            });

            if (response.ok) {
                resultDiv.innerText = `Konvertering från ${baseCurrency} till ${targetCurrency} sparades!`;
            } else {
                resultDiv.innerText = "Fel vid konvertering. Försök igen!";
            }
        } catch (error) {
            console.error("Kunde inte utföra konverteringen:", error);
        }
    });

    // Hämta växelkurser när sidan laddas
    fetchExchangeRates();
});
