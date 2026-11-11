export async function operation({ walk, sets }, optimize) {
  if (!Array.isArray(sets) || sets.some(set => typeof set.artist !== 'string' || !set.artist.trim())) {
    throw new TypeError('Each set needs an artist')
  }
  const { ids, totalWeight } = await optimize({ walk, sets })
  const byId = new Map(sets.map(set => [set.id, set]))
  const lineup = ids.map(id => byId.get(id))
  return { totalWeight, artists: lineup.map(set => set.artist), lineup }
}
