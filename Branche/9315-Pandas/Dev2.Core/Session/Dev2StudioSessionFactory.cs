namespace Dev2.Session {
    public static class Dev2StudioSessionFactory {

        /// <summary>
        /// Create a new studio session broker
        /// </summary>
        /// <returns></returns>
        public static IDev2StudioSessionBroker CreateBroker() {
            return new Dev2StudioSessionBroker();
        }
    }
}
