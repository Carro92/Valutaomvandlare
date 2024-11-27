async function fetchExchangeRates() {
    try {
        const response = await fetch(`${baseApiUrl}FetchExchangeRates`);
        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

        const data = await response.json();
        displayExchangeRates(data.rates);
    } catch (error) {
        console.error("Error fetching exchange rates:", error);
    }
}

function displayExchangeRates(rates) {
    const container = document.getElementById("exchangeRatesContainer");
    container.innerHTML = "";

    Object.entries(rates).forEach(([currency, rate]) => {
        const listItem = document.createElement("li");
        listItem.textContent = `${currency}: ${rate.toFixed(2)}`;
        container.appendChild(listItem);
    });
}
