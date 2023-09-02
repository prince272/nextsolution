const fs = require("fs").promises;
const devcert = require("devcert");

const keyPath = "localhost-key.pem";
const certPath = "localhost-cert.pem";

async function generateSSLCertificates() {
  try {
    const keyExists = await fs
      .access(keyPath)
      .then(() => true)
      .catch(() => false);
    const certExists = await fs
      .access(certPath)
      .then(() => true)
      .catch(() => false);

    if (!keyExists || !certExists) {
      if (keyExists) {
        await fs.unlink(keyPath);
        console.log(`${keyPath} deleted.`);
      }

      if (certExists) {
        await fs.unlink(certPath);
        console.log(`${certPath} deleted.`);
      }

      const { key, cert } = await devcert.certificateFor("localhost");

      await fs.writeFile(keyPath, key);
      console.log(`${keyPath} saved.`);

      await fs.writeFile(certPath, cert);
      console.log(`${certPath} saved.`);

      console.log("SSL certificates generated and saved successfully.");
    } else {
      console.log("SSL certificate already generated.");
    }
  } catch (error) {
    console.error("An error occurred with generating SSL certificate:", error);
  }
}

generateSSLCertificates();
