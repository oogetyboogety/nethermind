times=$1

cp benchmark.sh perf.sh

while read commit; do
  COUNTER=0
  while [  $COUNTER -lt $times ];
  do
    echo "Running $commit.$COUNTER.bench"
    ./perf.sh -d "C:/perf_db" > result.bench
    grep "TOTAL after 100000" result.bench > $commit.$COUNTER.bench
    grep Mgas/s $commit.$COUNTER.bench
    let COUNTER=$COUNTER+1
  done
done <commits.list

rm perf.sh