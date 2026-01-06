const api = require('@actual-app/api');

async function addTransactions(connectionInfo, accountId, transactions) {
  // Connect and sync up.
  await api.init({ serverURL: connectionInfo.serverUrl,  password: connectionInfo.serverPassword });
  await api.downloadBudget(connectionInfo.budgetSyncId);
  
  console.log("Add transaction query js");
  // Add the new transactions.
  await api.addTransactions(accountId, transactions);
  
  // Sync the new transactions to the server.
  // Temporary until api.shutdown() syncs, which will be with the next @actual-app/api release.
  await api.sync();
  
  // All done.
  await api.shutdown();
}

async function getCategories(connectionInfo) {
    // Connect and sync up.
    await api.init({ serverURL: connectionInfo.serverUrl, password: connectionInfo.serverPassword });
    await api.downloadBudget(connectionInfo.budgetSyncId);

    console.log("getCategories js");
    // Add the new transactions.
    let result = await api.getCategories();
    console.log(result);
    // Sync the new transactions to the server.
    // Temporary until api.shutdown() syncs, which will be with the next @actual-app/api release.
    //await api.internal.send('sync');

    // All done.
    await api.shutdown();

    return result;
}

module.exports = { addTransactions, getCategories };
