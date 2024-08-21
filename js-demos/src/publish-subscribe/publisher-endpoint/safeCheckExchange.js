import amqp from "amqplib";

function translateErrorToErrorCode(error) {
    if (error.message) {
      const splitString = error.message.split(' ');
      if (splitString.length >= 3) {
        return splitString[3];
      }
    }
    return null;
  }
  
  /**
   * Executes checkExchange function and deals with errors returning the
   * reply on success
   * @param {String} exchnage - the exchange to check
   * @returns {Promise.<*>}
   */
 export async function safeCheckExchange(exchnage) {

    const throwAwayConnection = await amqp.connect("amqp://localhost");
    const throwAwayChannel = await throwAwayConnection.createChannel();

    // listen to emitted error event for library because it doesn't
    throwAwayChannel.once('error', (error) => {
      // we only expect the check's 404 error
      if (error.code !== 404) {
        throw error;
      }
    });
  
    try {
      await throwAwayChannel.checkExchange(exchnage);
    } catch (error) {
      // since we only get a useful message, try to split it to get the error code
      const code = translateErrorToErrorCode(error);
      if (code === '404') {
        return Promise.resolve(false);
      }
      // if its not the 404 message something unexpected happened
      return Promise.reject(error);
    }
    // if the channel didn't blow up we should close it
    throwAwayChannel.close();
    return Promise.resolve(true);
  }