static class UID {
  private static int _id = 0;

  public static int Next() {
    return _id++;
  }
}