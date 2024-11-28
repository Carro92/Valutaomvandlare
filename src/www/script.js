const baseUrl = "https://valutaomvandlare-functionapp.azurewebsites.net/api/";

async function fetchExchangeRates() {
  try {
    const response = await fetch(`${baseUrl}FetchExchangeRates`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "x-functions-key": "eBd5dyeraiV5SCSFM9v9zjX5pandgP9st_LesloBGOTOAzFufvhS4w==", // Använd miljövariabel i production
      },
    });

    if (!response.ok) {
      throw new Error(`Error fetching exchange rates: ${response.status}`);
    }

    const data = await response.json();
    console.log("Exchange rates:", data);
    return data;
  } catch (error) {
    console.error("Error fetching exchange rates:", error);
    alert("Kunde inte hämta växelkurser. Kontrollera anslutningen.");
  }
}

async function convertCurrency(baseCurrency, targetCurrency, amount) {
  try {
    const response = await fetch(`${baseUrl}ConvertCurrency`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "x-functions-key": "eBd5dyeraiV5SCSFM9v9zjX5pandgP9st_LesloBGOTOAzFufvhS4w==", // Använd miljövariabel i production
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

    const data = await response.json();
    console.log("Conversion result:", data);
    return data;
  } catch (error) {
    console.error("Error converting currency:", error);
    alert("Kunde inte omvandla valutan. Kontrollera anslutningen.");
  }
}
