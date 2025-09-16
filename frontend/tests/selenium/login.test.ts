import { Builder, By, until, WebDriver } from "selenium-webdriver";
import chrome from "selenium-webdriver/chrome";

// Helper function to highlight an element
async function highlight(driver: WebDriver, element: any) {
  await driver.executeScript("arguments[0].style.border='3px solid red'", element);
}

async function register(driver: WebDriver) {
    await driver.get("http://localhost:5173/register");

    const nameInput = await driver.wait(until.elementLocated(By.css("#full-name")), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", nameInput);
    await highlight(driver, nameInput);
    await nameInput.sendKeys("dinethmin");

    const emailInput = await driver.wait(until.elementLocated(By.css("#email")), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", emailInput);
    await highlight(driver, emailInput);
    await emailInput.sendKeys("dm13@gmail.com");

    const passwordInput = await driver.wait(until.elementLocated(By.css("#password")), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", passwordInput);
    await highlight(driver, passwordInput);
    await passwordInput.sendKeys("Password123");

    const conpasswordInput = await driver.wait(until.elementLocated(By.css("#confirm-password")), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", conpasswordInput);
    await highlight(driver, conpasswordInput);
    await conpasswordInput.sendKeys("Password123");

    const termsInput = await driver.wait(until.elementLocated(By.css("#terms")), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", termsInput);
    await highlight(driver, termsInput);
    if (!(await termsInput.isSelected())) {
        await termsInput.click();
    }

    const submitBtn = await driver.wait(until.elementLocated(By.css('button[type="submit"]')), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", submitBtn);
    await highlight(driver, submitBtn);
    await driver.sleep(500);
    await driver.sleep(3000);
    await submitBtn.click();

    // Handle alert if present
    try {
        await driver.wait(until.alertIsPresent(), 5000);
        const alert = await driver.switchTo().alert();
        console.log("Alert detected: " + (await alert.getText()));
        await alert.accept();
    } catch {
        console.log("No alert appeared");
    }

    // Wait for login page after registration
    await driver.wait(until.urlContains("/login"), 10000);
    console.log("✅ Registration Test Passed");
}

async function login(driver: WebDriver) {
    await driver.get("http://localhost:5173/login");

    const emailInput = await driver.wait(until.elementLocated(By.css("#email")), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", emailInput);
    await highlight(driver, emailInput);
    await emailInput.sendKeys("dm13@gmail.com");

    const passwordInput = await driver.wait(until.elementLocated(By.css("#password")), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", passwordInput);
    await highlight(driver, passwordInput);
    await passwordInput.sendKeys("Password123");
    await driver.sleep(1000);

    const submitBtn = await driver.wait(until.elementLocated(By.css('button[type="submit"]')), 10000);
    await driver.executeScript("arguments[0].scrollIntoView(true);", submitBtn);
    await highlight(driver, submitBtn);
    await driver.sleep(500);
    await driver.sleep(3000);
    await submitBtn.click();

    

    // Wait for dashboard page
    await driver.wait(until.urlContains("/owner"), 10000);
    console.log("✅ Login Test Passed");

    // Keep browser open for 3 seconds before closing
    await driver.sleep(3000);
}

async function runTests() {
    const options = new chrome.Options();
    const driver = await new Builder().forBrowser("chrome").setChromeOptions(options).build();

    try {
        await register(driver);
        await login(driver);
    } catch (err) {
        console.error("❌ Test Failed:", err);
    } finally {
        await driver.quit(); // Close browser at the end
    }
}

runTests();
